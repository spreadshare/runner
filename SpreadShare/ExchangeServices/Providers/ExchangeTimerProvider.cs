using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Exchange implementation of the TimerProvider.
    /// </summary>
    internal class ExchangeTimerProvider : TimerProvider
    {
        private int _consecutiveExceptions = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        public ExchangeTimerProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            // Set the pivot point to midnight.
            // Pivot = new DateTimeOffset(2018, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            Pivot = DateTimeOffset.FromUnixTimeSeconds(0);
        }

        /// <inheritdoc />
        public override DateTimeOffset CurrentTime => DateTimeOffset.Now;

        /// <inheritdoc />
        public override DateTimeOffset Pivot { get; }

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
                SwitchType.Case<OrderRefusedException>(() => Program.ExitProgramWithCode(ExitCode.OrderFailure)),
                SwitchType.Case<OrderFailedException>(() => Program.ExitProgramWithCode(ExitCode.OrderFailure)),
                SwitchType.Case<ExchangeConnectionException>(() => _consecutiveExceptions++),
                SwitchType.Case<ProviderException>(() => _consecutiveExceptions++),
                SwitchType.Default(() => _consecutiveExceptions++));
        }
    }
}