using System;
using System.Threading.Tasks;
using Dawn;
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
        private DateTimeOffset _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="startDate">The starting moment of the backtest (in UTC)</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DateTimeOffset startDate)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            CurrentTime = startDate;
            _target = DateTimeOffset.FromUnixTimeMilliseconds(1540198260000);
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public DateTimeOffset CurrentTime { get; private set; }

        /// <inheritdoc />
        public override void SetTimer(uint minutes, Action callback)
        {
            Guard.Argument(callback).NotNull();
            CurrentTime += TimeSpan.FromMinutes(minutes);
            _logger.LogInformation($"Skipping time for {minutes} minutes, new time: {CurrentTime.ToUniversalTime()}");
        }

        /// <inheritdoc />
        public override void StopTimer()
        {
            _logger.LogWarning("Backtesting timer was stopped, but this has no effect, as the timer elapses instantly");
        }

        /// <inheritdoc />
        public async override void RunPeriodicTimer()
        {
            // Make sure all constructor processes are finished
            await Task.Delay(1000).ConfigureAwait(false);
            DateTimeOffset start = DateTimeOffset.Now;
            while (CurrentTime < _target)
            {
                _logger.LogInformation($"It is now {CurrentTime}");
                CurrentTime += TimeSpan.FromMinutes(1);
                UpdateObservers(CurrentTime.ToUnixTimeMilliseconds());
                await Task.Delay(200).ConfigureAwait(false);
            }

            _logger.LogCritical($"STOP THE TIMERS! Backtest took {(DateTimeOffset.Now - start).TotalMilliseconds}ms");
        }
    }
}