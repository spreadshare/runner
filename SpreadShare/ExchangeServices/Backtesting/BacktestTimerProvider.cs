using System;
using Microsoft.EntityFrameworkCore.Design;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="startDate">The starting moment of the backtest (in UTC)</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DateTimeOffset startDate)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            CurrentTime = startDate;
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public DateTimeOffset CurrentTime { get; private set; }

        public long CurrentEpoc => CurrentTime.ToUnixTimeMilliseconds();

        /// <inheritdoc />
        public void SetTimer(uint minutes, Action callback)
        {
            CurrentTime += TimeSpan.FromMinutes(minutes);
            _logger.LogInformation($"Skipping time for {minutes} minutes, new time: {CurrentTime.ToUniversalTime()}");
        }

        /// <inheritdoc />
        public void StopTimer()
        {
            throw new NotImplementedException();
        }
    }
}