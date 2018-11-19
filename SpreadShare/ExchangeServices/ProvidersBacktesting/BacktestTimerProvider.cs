using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

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
        private DatabaseContext _database;
        private readonly string _outputFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="database">The database context for flushing</param>
        /// <param name="startDate">The starting moment of the backtest (in UTC)</param>
        /// <param name="endDate">Runs the timer till end date</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestSettings settings)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _database = database;
            _currentTime = DateTimeOffset.FromUnixTimeMilliseconds(settings.BeginTimeStamp) + TimeSpan.FromHours(48);
            _endDate = DateTimeOffset.FromUnixTimeMilliseconds(settings.EndTimeStamp);
            _outputFolder = settings.OutputFolder;
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public override DateTimeOffset CurrentTime => _currentTime;

        /// <inheritdoc />
        public override async void RunPeriodicTimer()
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
            _logger.LogCritical("Flushing the trades to the database...");
            _database.SaveChanges();
            _logger.LogCritical("...DONE");
            var outputLogger = new BacktestOutputLogger(_database, _outputFolder);
            outputLogger.Output();
        }
    }
}