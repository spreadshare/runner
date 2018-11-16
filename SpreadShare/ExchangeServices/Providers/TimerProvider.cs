using System;
using SpreadShare.ExchangeServices.Providers.Observing;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract definition of the timer provider
    /// This object is used inside an ExchangeProviderContainer
    /// </summary>
    internal abstract class TimerProvider : Observable<long>
    {
        /// <summary>
        /// Gets the current time
        /// </summary>
        public abstract DateTimeOffset CurrentTime { get; }

        /// <summary>
        /// Notify the observers periodically
        /// </summary>
        public abstract void RunPeriodicTimer();
    }
}