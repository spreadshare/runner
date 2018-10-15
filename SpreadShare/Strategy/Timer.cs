using System;
using Cron;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// A wrapper for the System.Thread.Timer class that doesn't fail on long waiting times.
    /// WARNING: This timer has an inprecision of 60 seconds.
    /// </summary>
    internal class Timer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Action _callback;
        private readonly CronDaemon _cronDaemon = new CronDaemon();
        private readonly uint _target;
        private uint _counter = 0;
        private bool _executed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// The timer will be started right away and run for AT LEAST the amount
        /// of minutes you give it.
        /// </summary>
        /// <param name="minutes">Waiting time in minutes</param>
        /// <param name="factory">LoggerFactory for creating logging output</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(uint minutes, ILoggerFactory factory, Action callback)
        {
            _callback = callback ?? throw new ArgumentException("Callback can't be null");

            _logger = factory.CreateLogger<Timer>();
            _target = minutes;

            // Create a Cron Deamon that executes every minute;
            _cronDaemon.Add("* * * * *", Execute);
            _cronDaemon.Start();
        }

        /// <summary>
        /// Makes sure the callback is never executed
        /// </summary>
        public void Stop()
        {
            _executed = true;
            if (!_executed)
            {
                _logger.LogInformation("Stopped");
            }
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
                _executed = true;
                _cronDaemon.Stop();
            }
        }

        /// <summary>
        /// Wrapper function for the callback method.
        /// </summary>
        private void Execute()
        {
            if (_executed)
            {
                return;
            }

            if (_counter < _target)
            {
                _logger.LogInformation($"Call minute #{_counter++}/{_target}    {DateTime.UtcNow}");
                return;
            }

            _logger.LogInformation("Executing Callback");
            _executed = true;
            _callback();
        }
    }
}