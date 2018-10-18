using System;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Abstract defnition of the timer provider
    /// This object is used inside an ExchangeProviderContainer
    /// </summary>
    internal abstract class TimerProvider
    {
        /// <summary>
        /// Set a timer before executing a certain callback
        /// </summary>
        /// <param name="minutes">The amount of minutes to wait</param>
        /// <param name="callback">The method to execute after given time</param>
        public abstract void SetTimer(uint minutes, Action callback);
    }
}