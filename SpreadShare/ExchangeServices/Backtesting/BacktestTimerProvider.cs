using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;

namespace SpreadShare.ExchangeServices.Backtesting
{
    /// <summary>
    /// Mocking implementation of a timer service for backtesting.
    /// </summary>
    internal class BacktestTimerProvider : ITimerProvider
    {
        private readonly ILogger _logger;
        private DateTime _currentTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public void SetTimer(uint minutes, Action callback)
        {
            _currentTime += TimeSpan.FromMinutes(minutes);
            _logger.LogInformation($"Skipping time for {minutes} minutes, new time: {_currentTime.ToUniversalTime()}");
        }

        /// <inheritdoc />
        public void StopTimer()
        {
            throw new NotImplementedException();
        }
    }
}