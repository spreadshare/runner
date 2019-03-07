using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Provider implementation for backtest purposes.
    /// </summary>
    internal class BacktestDataProvider : AbstractDataProvider
    {
        private readonly BacktestTimerProvider _timer;
        private readonly int _millisecondsCandleWidth;
        private readonly BacktestBuffers _buffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="database">The backtest database database.</param>
        /// <param name="timerProvider">Used to keep track of time.</param>
        /// <param name="backtestCommunicationService">Communicates with backtesting and provides order updates.</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestTimerProvider timerProvider, BacktestCommunicationService backtestCommunicationService)
            : base(loggerFactory, backtestCommunicationService)
        {
            _timer = timerProvider;

            // Calculate once.
            _millisecondsCandleWidth = (int)TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth).TotalMilliseconds;
            _buffers = new BacktestBuffers(database, Logger);
        }

        /// <summary>
        /// Sets the DataProvider that implements this BackTestTradingProvider.
        /// </summary>
        public DataProvider ParentImplementation { private get; set; }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            var (candle, _) = FindCandle(pair, _timer.CurrentTime.ToUnixTimeMilliseconds());
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair)
        {
            // Ask for the open of one candle in the future
            var timestamp = (_timer.CurrentTime + TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth)).ToUnixTimeMilliseconds();
            var (candle, _) = FindCandle(pair, timestamp);
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Open);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => GetCurrentPriceTopBid(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            long timestamp = _timer.CurrentTime.ToUnixTimeMilliseconds();
            var (candleNow, _) = FindCandle(pair, timestamp);
            var (candleBack, _) = FindCandle(pair, timestamp - (long)(hoursBack * 3600L * 1000L));

            return new ResponseObject<decimal>(ResponseCode.Success, candleNow.Average / candleBack.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width)
        {
            var time = _timer.CurrentTime;
            var result = new BacktestingCandle[limit];
            for (int i = 0; i < limit; i++)
            {
                (result[i], _) = FindCandle(pair, time.ToUnixTimeMilliseconds());
                time -= TimeSpan.FromMinutes(5);
            }

            return new ResponseObject<BacktestingCandle[]>(result);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetHighestHigh(TradingPair pair, CandleWidth width, int numberOfCandles)
        {
            var (_, index) = FindCandle(pair, _timer.CurrentTime.ToUnixTimeMilliseconds());
            return new ResponseObject<decimal>(_buffers.GetHighestHighs(pair, numberOfCandles)[index]);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetLowestLow(TradingPair pair, CandleWidth width, int numberOfCandles)
        {
            var (_, index) = FindCandle(pair, _timer.CurrentTime.ToUnixTimeMilliseconds());
            return new ResponseObject<decimal>(_buffers.GetLowestLow(pair, numberOfCandles)[index]);
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            decimal max = -1;
            TradingPair maxTradingPair = null;

            foreach (var tradingPair in pairs)
            {
                var performanceQuery = GetPerformancePastHours(tradingPair, hoursBack);
                decimal performance;
                if (performanceQuery.Code == ResponseCode.Success)
                {
                    performance = performanceQuery.Data;
                }
                else
                {
                    Logger.LogWarning($"Error fetching performance data: {performanceQuery}");
                    return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Error, performanceQuery.ToString());
                }

                if (max < performance)
                {
                    max = performance;
                    maxTradingPair = tradingPair;
                }
            }

            if (maxTradingPair == null)
            {
                return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Error, "No trading pairs defined");
            }

            return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Success, new Tuple<TradingPair, decimal>(maxTradingPair, max));
        }

        /// <summary>
        /// Get a value of the portfolio using the parent wrapper.
        /// </summary>
        /// <param name="portfolio">Portfolio to evaluate.</param>
        /// <returns>value of portfolio in base currency.</returns>
        public decimal ValuatePortfolioInBaseCurrency(Portfolio portfolio)
        {
            return ParentImplementation.ValuatePortfolioInBaseCurrency(portfolio);
        }

        /// <summary>
        /// Find candle that matches the timestamp most closely.
        /// </summary>
        /// <param name="pair">Candle's trading pair.</param>
        /// <param name="timestamp">CreatedTimestamp to match.</param>
        /// <returns>Candle matching timestamp most closely and the index at which it was encountered.</returns>
        private (BacktestingCandle, long) FindCandle(TradingPair pair, long timestamp)
        {
            var buffer = _buffers.GetCandles(pair);

            // Minus one to prevent reading candles whose close is in the future.
            long index = ((timestamp - buffer[0].Timestamp) / _millisecondsCandleWidth) - 1;
            if (index < 0)
            {
                Logger.LogError("Got request for a candle that exists before the scope of available data," +
                                "Did you use GetTopPerformance without allowing enough time offset?");
                throw new InvalidOperationException("Tried to read outside backtest data buffer");
            }

            return (buffer[index], index);
        }
    }
}