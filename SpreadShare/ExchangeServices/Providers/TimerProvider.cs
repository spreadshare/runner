using System;
using System.Threading.Tasks;
using SpreadShare.ExchangeServices.Providers.Observing;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract defnition of the timer provider
    /// This object is used inside an ExchangeProviderContainer
    /// </summary>
    /// TODO: Make sure that periodic signals are send out to the observers.
    internal abstract class TimerProvider : Observable<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerProvider"/> class.
        /// </summary>
        protected TimerProvider()
        {
            RunPeriodicTimer();
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        /// <returns>Current UTC time</returns>
        public abstract DateTimeOffset GetCurrentTime();

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
        /// Notifies the observers every few seconds
        /// </summary>
        private async void RunPeriodicTimer()
        {
            while (true)
            {
                UpdateObservers(GetCurrentTime().ToUnixTimeMilliseconds());
                await Task.Delay(2000).ConfigureAwait(false);
            }
        }
    }
}