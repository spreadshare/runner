using System;
using System.Threading;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Provides timer functionality
    /// </summary>
    internal class Timer : IDisposable
    {
        private System.Threading.Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time; can't be negative</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(long ms, Action callback)
        {
            if (ms < 0)
            {
                throw new ArgumentException("Argument 'ms' can't be negative.");
            }

            int count = 0;
            int targetCount = (int)(ms / 1000.0);

            void Func()
            {
                Console.WriteLine($"Hoi {count++} | {DateTime.UtcNow}");
                if (count >= targetCount)
                {
                    _timer = new System.Threading.Timer((_) => callback(), null, (int)(ms % 1000), Timeout.Infinite);
                }
            }

            _timer = new System.Threading.Timer(
                _ => Func(),
                null,
                1000,
                1000);
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object's resource
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }
    }
}