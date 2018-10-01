using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadShare.Strategy
{
    internal class Timer : IDisposable {
        private readonly long _endTime;
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

            _shouldStop = false;
            _callback = callback ?? throw new ArgumentException("Argument callback can't be null.");
            _endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ms;
            WaitAsync();
        }

        /// <summary>
        /// Execute callback after the timer is finished or exit prematurely
        /// </summary>
        private async Task WaitAsync() {
            using(this) {
                while(DateTimeOffset.Now.ToUnixTimeMilliseconds() < _endTime) {
                    if (_shouldStop) {
                        Console.WriteLine("TIMER GOT INTERRUPTED");
                        return;
                    }
                    await Task.Delay(1);
                }
                _callback();
            }
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
        }

        public void Dispose() { }
    }
}