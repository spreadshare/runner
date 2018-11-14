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
        private CronDaemon _daemon;
        private Action _callback;
        private uint _targetCount;

        private uint _count;

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
            Guard.Argument(callback);
            _callback = callback;

            // Set starting values
            _count = 0;
            _targetCount = minutes;

            // Create cron timer with one minute time interval.
            _daemon = new CronDaemon();
            _daemon.Add("* * * * * *", Execute);
            _daemon.Start();
        }

        /// <inheritdoc />
        public override void StopTimer()
        {
            _daemon.Stop();
        }

        /// <summary>
        /// Notifies the observer periodically
        /// </summary>
        protected override async void RunPeriodicTimer()
        {
            while (true)
            {
                UpdateObservers(GetCurrentTime().ToUnixTimeMilliseconds());
                await Task.Delay(2000).ConfigureAwait(false);
            }
        }

        private void Execute()
        {
            if (_count++ < _targetCount)
            {
                return;
            }

            _daemon.Stop();
            _callback();
        }
    }
}