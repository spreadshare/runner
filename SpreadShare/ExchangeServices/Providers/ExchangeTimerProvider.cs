using System;
using Cron;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Exchange implementation of the TimerProvider
    /// </summary>
    internal class ExchangeTimerProvider : ITimerProvider
    {
        private CronDaemon _daemon;
        private Action _callback;
        private uint _targetCount;

        private uint _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimerProvider"/> class.
        /// </summary>
        public ExchangeTimerProvider()
        {
        }

        /// <summary>
        /// Set a timer before executing a callback.
        /// Has an inaccuracy of 60 seconds.
        /// </summary>
        /// <param name="minutes">the minimum amount of minutes to wait</param>
        /// <param name="callback">the method to execute after given time</param>
        public void SetTimer(uint minutes, Action callback)
        {
            _callback = callback ?? throw new ArgumentException("Callback can't be null");

            // Set starting values
            _count = 0;
            _targetCount = minutes;

            // Create cron timer with one minute time interval.
            _daemon = new CronDaemon();
            _daemon.Add("* * * * * *", Execute);
            _daemon.Start();
        }

        /// <inheritdoc />
        public void StopTimer()
        {
            _daemon.Stop();
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