using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Exchange implementation of the TimerProvider.
    /// </summary>
    internal class ExchangeTimerProvider : TimerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        public ExchangeTimerProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        /// <inheritdoc />
        public override DateTimeOffset CurrentTime => DateTimeOffset.Now;

        /// <summary>
        /// Notifies the observer periodically.
        /// </summary>
        public override async void RunPeriodicTimer()
        {
            int consecutiveExceptions = 0;
            while (true)
            {
                try
                {
                    UpdateObservers(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    consecutiveExceptions = 0;
                }
                catch (ArgumentException e)
                {
                    Logger.LogError(e, e.Message);
                    Program.ExitProgramWithCode(ExitCode.UnexpectedValue);
                }
                catch (OrderRefusedException e)
                {
                    Logger.LogError(e, e.Message);
                    Program.ExitProgramWithCode(ExitCode.OrderFailure);
                }
                catch (OrderFailedException e)
                {
                    Logger.LogError(e, e.Message);
                    Program.ExitProgramWithCode(ExitCode.OrderFailure);
                }
                catch (ExchangeConnectionException e)
                {
                    Logger.LogError(e, e.Message);
                    consecutiveExceptions++;
                }
                catch (ProviderException e)
                {
                    Logger.LogError(e, e.Message);
                    consecutiveExceptions++;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
                    consecutiveExceptions++;
                }

                if (consecutiveExceptions > 0)
                {
                    if (consecutiveExceptions >= 5)
                    {
                        Logger.LogError($"Got {consecutiveExceptions} consecutive exceptions, shutting down.");
                        Program.ExitProgramWithCode(ExitCode.ConsecutiveExceptionFailure);
                    }

                    var coolDown = TimeSpan.FromMilliseconds(10000 * Math.Pow(2, consecutiveExceptions));
                    Logger.LogWarning($"Continuing program after {coolDown}");
                    await Task.Delay((int)coolDown.TotalMilliseconds).ConfigureAwait(false);
                }

                await Task.Delay(5000).ConfigureAwait(false);
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}