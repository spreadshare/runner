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
        /// Gets the last 5 minute candle close timestamp.
        /// </summary>
        public abstract DateTimeOffset LastCandleOpen { get; }

        /// <summary>
        /// Gets the pivot, i.e. the starting point of candle creation.
        /// </summary>
        public abstract DateTimeOffset Pivot { get; }

        /// <summary>
        /// Gets a logger to log errors and trigger sentry events.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Notify the observers periodically.
        /// </summary>
        public abstract void RunPeriodicTimer();

        /// <summary>
        /// Method that terminates when a new candle has gone in.
        /// </summary>
        public abstract void WaitForNextCandle();
    }
}