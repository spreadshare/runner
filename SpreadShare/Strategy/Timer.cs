using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadShare.Strategy
{
    internal class Timer 
    {
        private System.Threading.Timer _timer;

        /// <summary>
        /// Constructor: Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time; can't be negative</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(long ms, Action callback) {
            if (ms < 0) throw new ArgumentException("Argument 'ms' can't be negative.");
            _timer = new System.Threading.Timer(
                (_) => { callback(); },
                null,
                ms,
                Timeout.Infinite
            );
        }
        
        public void Stop() {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}