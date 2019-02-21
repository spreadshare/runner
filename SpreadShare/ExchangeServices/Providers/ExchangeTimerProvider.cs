using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        public async override void RunPeriodicTimer()
        {
            while (true)
            {
                try
                {
                    UpdateObservers(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
                    break;
                }

                await Task.Delay(2000).ConfigureAwait(false);
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}