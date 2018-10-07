using System;
using System.Threading;

namespace SpreadShare.Strategy
{
    internal class Timer : IDisposable
    {
        private System.Threading.Timer _timer;

        /// <summary>
        /// Constructor: Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time; can't be negative</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(long ms, Action callback) {
            if (ms < 0) throw new ArgumentException("Argument 'ms' can't be negative.");

            int count = 0;
            int targetCount = (int)(ms / 1000.0);
            Action func = () => {
                Console.WriteLine($"Hoi {count++} | {DateTime.UtcNow}");
                if (count >= targetCount)
                {
                    _timer = new System.Threading.Timer(
                        (_) => callback(),
                        null,
                        (int)(ms%1000),
                        Timeout.Infinite
                    );
                }
            };

            _timer = new System.Threading.Timer(
                (_) => func(),
                null,
                1000,
                1000
            );
        }

        
        public void Stop() {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}