using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace SpreadShare.Strategy
{
    internal class Timer {
        private System.Timers.Timer _timer;


        //public bool Valid => !_shouldStop && DateTimeOffset.Now.ToUnixTimeMilliseconds() < _endTime;

        /// <summary>
        /// Constructor: Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time; can't be negative</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(long ms, Action callback) {
            if (ms < 0) throw new ArgumentException("Argument 'ms' can't be negative.");
            _timer = new System.Timers.Timer(ms);
            _timer.Elapsed += (obj, args) => callback();
            _timer.AutoReset = false;
            _timer.Start();
        }

        
        public void Stop() {
            _timer.Stop();
        }
    }
}