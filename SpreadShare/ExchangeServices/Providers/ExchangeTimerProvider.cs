using System;
using System.Threading.Tasks;
using Cron;
using Dawn;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Exchange implementation of the TimerProvider
    /// </summary>
    internal class ExchangeTimerProvider : TimerProvider
    {
        /// <inheritdoc />
        public override DateTimeOffset GetCurrentTime() => DateTimeOffset.UtcNow;

        /// <summary>
        /// Set a timer before executing a callback.
        /// Has an inaccuracy of 60 seconds.
        /// </summary>
        /// <param name="minutes">the minimum amount of minutes to wait</param>
        /// <param name="callback">the method to execute after given time</param>
        public override void SetTimer(uint minutes, Action callback)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void StopTimer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies the observer periodically
        /// </summary>
        public override async void RunPeriodicTimer()
        {
            while (true)
            {
                UpdateObservers(GetCurrentTime().ToUnixTimeMilliseconds());
                await Task.Delay(2000).ConfigureAwait(false);
            }
        }
    }
}