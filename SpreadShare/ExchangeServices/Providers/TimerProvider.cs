using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers.Observing;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract definition of the timer provider
    /// This object is used inside an ExchangeProviderContainer.
    /// </summary>
    internal abstract class TimerProvider : Observable<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">To create output.</param>
        protected TimerProvider(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets the current time.
        /// </summary>
        public abstract DateTimeOffset CurrentTime { get; }

        /// <summary>
        /// Gets a logger to log errors and trigger sentry events.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Notify the observers periodically.
        /// </summary>
        public abstract void RunPeriodicTimer();
    }
}