using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Database;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Exchange implementation of the TimerProvider.
    /// </summary>
    internal class BinanceTimerProvider : TimerProvider
    {
        private readonly ILogger _logger;
        private int _consecutiveExceptions = 0;
        private DataProvider _dataProvider;
        private DateTimeOffset _candleOpenTimestamp;
        private volatile bool _candleCloseFlag;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="comms">The communication service.</param>
        public BinanceTimerProvider(ILoggerFactory loggerFactory, BinanceCommunicationsService comms)
            : base(loggerFactory)
        {
            // Set the pivot point to midnight.
            Pivot = DateTimeOffset.FromUnixTimeSeconds(0);
            _logger = loggerFactory.CreateLogger(GetType());
            _candleOpenTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(0);
            comms.CandleDispenser.Subscribe(new ConfigurableObserver<BacktestingCandle>(
                () => { }, _ => { }, candle =>
                {
                    _candleOpenTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(candle.OpenTimestamp);

                    // Get the last compressed candle from the data provider to determine if a compressed candle is closed.
                    var lastCandle = DataProvider.GetCandles(TradingPair.Parse(candle.TradingPair), 1).Last();
                    var lastCandleCloseTimestamp = lastCandle.OpenTimestamp + (Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration.CandleWidth * 60 * 1000);
                    var currentCandleCloseTimestamp = candle.OpenTimestamp + (Configuration.Instance.CandleWidth * 60 * 1000);
                    _logger.LogDebug($"StateManager: Socket Candle Time: {DateTimeOffset.FromUnixTimeMilliseconds(currentCandleCloseTimestamp)} - Compressed Rest API Candle Time: {DateTimeOffset.FromUnixTimeMilliseconds(lastCandleCloseTimestamp)} - Diff: {lastCandleCloseTimestamp - currentCandleCloseTimestamp}");
                    if (lastCandleCloseTimestamp == currentCandleCloseTimestamp)
                    {
                        _logger.LogInformation($"The candle has just been closed!");
                        _candleCloseFlag = true;
                    }
                }));
        }

        /// <inheritdoc />
        public override DateTimeOffset CurrentTime => DateTimeOffset.Now;

        /// <inheritdoc/>
        public override DateTimeOffset LastCandleOpen => _candleOpenTimestamp;

        /// <inheritdoc />
        public override DateTimeOffset Pivot { get; }

        /// <summary>
        /// Sets a <see cref="DataProvider"/> instance, the setter is used via reflection in <see cref="ExchangeFactoryService"/>.
        /// </summary>
        public DataProvider DataProvider
        {
            private get => _dataProvider;
            set
            {
                if (_dataProvider != null)
                {
                    throw new InvalidOperationException("Cannot set the value of the DataProvider more than once.");
                }

                _dataProvider = value;
            }
        }

        /// <inheritdoc />
        public override void WaitForNextCandle()
        {
            _logger.LogDebug("Stalling program until next candle close.");
            _candleCloseFlag = false;
            while (!_candleCloseFlag)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Notifies the observer periodically.
        /// </summary>
        public override async void RunPeriodicTimer()
        {
            while (true)
            {
                try
                {
                    UpdateObservers(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    _consecutiveExceptions = 0;
                }
                catch (Exception e)
                {
                    HandleException(e);
                }

                if (_consecutiveExceptions > 0)
                {
                    if (_consecutiveExceptions >= 5)
                    {
                        Logger.LogError($"Got {_consecutiveExceptions} consecutive exceptions, shutting down.");
                        Program.ExitProgramWithCode(ExitCode.ConsecutiveExceptionFailure);
                    }

                    var coolDown = TimeSpan.FromMilliseconds(30000 * Math.Pow(2, _consecutiveExceptions));
                    Logger.LogWarning($"Continuing program after {coolDown}");
                    await Task.Delay((int)coolDown.TotalMilliseconds).ConfigureAwait(false);
                }

                await Task.Delay(5000).ConfigureAwait(false);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private void HandleException(Exception e)
        {
            Logger.LogError(e, e.Message);
            e.Switch(
                SwitchType.Case<TargetInvocationException>(() => HandleException(e.InnerException)),
                SwitchType.Case<ArgumentException>(() => Program.ExitProgramWithCode(ExitCode.UnexpectedValue)),
                SwitchType.Case<OutOfFundsException>(() => Program.ExitProgramWithCode(ExitCode.OrderFailure)),
                SwitchType.Case<OrderRefusedException>(() => Program.ExitProgramWithCode(ExitCode.OrderFailure)),
                SwitchType.Case<OrderFailedException>(() => Program.ExitProgramWithCode(ExitCode.OrderFailure)),
                SwitchType.Case<ExchangeConnectionException>(() => _consecutiveExceptions++),
                SwitchType.Case<ProviderException>(() => _consecutiveExceptions++),
                SwitchType.Default(() => _consecutiveExceptions++));
        }
    }
}