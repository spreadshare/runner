using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Mocking implementation of a timer service for backtesting.
    /// </summary>
    internal class BacktestTimerProvider : TimerProvider
    {
        private readonly ILogger _logger;
        private readonly DateTimeOffset _endDate;
        private DateTimeOffset _currentTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="startDate">The starting moment of the backtest (in UTC)</param>
        /// <param name="endDate">Runs the timer till end date</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _currentTime = startDate;
            _endDate = endDate;
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public override DateTimeOffset CurrentTime => _currentTime;

        /// <inheritdoc />
        public async override void RunPeriodicTimer()
        {
            // Make sure all constructor processes are finished
            await Task.Delay(1000).ConfigureAwait(false);
            DateTimeOffset start = DateTimeOffset.Now;
            while (CurrentTime < _endDate)
            {
                _currentTime += TimeSpan.FromMinutes(1);
                UpdateObservers(CurrentTime.ToUnixTimeMilliseconds());
            }

            _logger.LogCritical($"STOP THE TIMERS! Backtest took {(DateTimeOffset.Now - start).TotalMilliseconds}ms");
        }
    }
}