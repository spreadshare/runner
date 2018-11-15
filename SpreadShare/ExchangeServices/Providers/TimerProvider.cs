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
        /// Set a timer before executing a certain callback
        /// </summary>
        /// <param name="minutes">The amount of minutes to wait</param>
        /// <param name="callback">The method to execute after given time</param>
        public abstract void SetTimer(uint minutes, Action callback);

        /// <summary>
        /// Stop the timer
        /// </summary>
        public abstract void StopTimer();

        /// <summary>
        /// Notify the observers periodically
        /// </summary>
        public abstract void RunPeriodicTimer();
    }
}