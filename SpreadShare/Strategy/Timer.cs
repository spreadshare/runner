using System;
using System.Threading;
using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// A wrapper for the System.Thread.Timer class that doesn't fail on long waiting times.
    /// </summary>
    internal class Timer : IDisposable
    {
        private readonly Action _callback;
        private readonly System.Threading.Timer _timer;
        private readonly uint _targetCount;
        private readonly int _rest;
        private uint _counter = 0;
        private bool _executed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// The timer will be started right away.
        /// </summary>
        /// <param name="ms">Waiting time</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(uint ms, Action callback)
        {
            _callback = callback ?? throw new ArgumentException("Callback can't be null");

            _targetCount = ms / 1000;
            _rest = (int)(ms % 1000);

            // As suggested by https://adrientorris.github.io/aspnet-core/how-to-implement-timer-netcoreapp1-0-netcoreapp1-1.html
            var autoEvent = new AutoResetEvent(false);
            _timer = new System.Threading.Timer(Execute, autoEvent, 0, 1000);
        }

        /// <summary>
        /// Makes sure the callback is never executed
        /// </summary>
        public void Stop()
        {
            _executed = true;
        }

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
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
        }

        /// <summary>
        /// Wrapper function for the callback method.
        /// </summary>
        /// <param name="stateInfo">Provided by the System EventHandler</param>
        private async void Execute(object stateInfo)
        {
            if (_executed)
            {
                return;
            }

            if (_counter < _targetCount)
            {
                Console.WriteLine($"Call #{_counter++}    {DateTime.UtcNow}");
                return;
            }

            if (_rest > 0)
            {
                await Task.Delay(_rest);
            }

            Console.WriteLine("Executing Callback");
            _executed = true;
            _callback();
        }
    }
}