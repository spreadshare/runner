using System;
using System.Threading;

namespace SpreadShare.Strategy
{
    internal class Timer {
        private readonly long _endTime;
        private readonly Thread _thread;
        private readonly Action _callback;
        private bool _shouldStop;

        public bool Valid => !_shouldStop && DateTimeOffset.Now.ToUnixTimeMilliseconds() < _endTime;

        /// <summary>
        /// Constructor: Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time; can't be negative</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(long ms, Action callback) {
            if (ms < 0) throw new ArgumentException("Argument 'ms' can't be negative.");

            _callback = callback ?? throw new ArgumentException("Argument callback can't be null.");
            _endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ms;
            _thread = new Thread(Wait);
            _thread.Start();
        }

        /// <summary>
        /// Execute callback after the timer is finished or exit prematurely
        /// </summary>
        private void Wait() {
            while(DateTimeOffset.Now.ToUnixTimeMilliseconds() < _endTime) {
                if (_shouldStop)
                    return;
                Thread.Sleep(1);
            }
            _callback();
        }

        /// <summary>
        /// Get remaining amount of time of the timer
        /// </summary>
        /// <returns>Remaining ms of the timer; -1 if finished</returns>
        public long GetRemaining()
        {
            var remaining = _endTime - DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return remaining < 0 ? -1 : remaining;
        }

        /// <summary>   
        /// Stops the timer
        /// </summary>
        public void Stop() {
            _shouldStop = true;
            if (_thread.IsAlive) _thread.Join();
        }
    }
}