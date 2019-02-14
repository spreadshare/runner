using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.BacktestDaemon;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Mocking implementation of a timer service for backtesting.
    /// </summary>
    internal class BacktestTimerProvider : TimerProvider
    {
        private readonly ILogger _logger;
        private readonly DatabaseContext _database;
        private readonly string _outputFolder;
        private DateTimeOffset _currentTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="database">The database context for flushing.</param>
        /// <param name="settings">Provides startDate, endDate and outputFolder.</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestSettings settings)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _database = database;

            // Hardcoded 2 week offset
            BeginTime = DateTimeOffset.FromUnixTimeMilliseconds(
                BacktestDaemonService.Instance.State.BeginTimeStamp) + TimeSpan.FromDays(14);
            _currentTime = BeginTime;
            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(
                BacktestDaemonService.Instance.State.EndTimeStamp);
            _outputFolder = settings.OutputFolder;
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public override DateTimeOffset CurrentTime => _currentTime;

        /// <summary>
        /// Gets the date at which the backtest started.
        /// </summary>
        public DateTimeOffset BeginTime { get; }

        /// <summary>
        /// Gets the date at which the backtest ended.
        /// </summary>
        public DateTimeOffset EndTime { get; }

        /// <summary>
        /// Gets a value indicating whether the backtest is finished.
        /// </summary>
        public bool Finished { get; private set; }

        /// <summary>
        /// Gets or set a tuple indicating that there was an error and a message.
        /// </summary>
        public (bool, Exception) ErrorRegister { get; private set; }

        /// <inheritdoc />
        public override async void RunPeriodicTimer()
        {
            // Make sure all constructor processes are finished
            await Task.Delay(1000).ConfigureAwait(false);

            // Clear the trades and state switch event table
            _database.Database.ExecuteSqlCommand("TRUNCATE TABLE public.\"Trades\"");
            _database.Database.ExecuteSqlCommand("TRUNCATE TABLE public.\"StateSwitchEvents\"");
            _database.SaveChanges();

            Console.WriteLine($"From {BeginTime} to {EndTime}");

            DateTimeOffset start = DateTimeOffset.Now;
            while (CurrentTime < EndTime)
            {
                try
                {
                    _currentTime += TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth);
                    UpdateObservers(CurrentTime.ToUnixTimeMilliseconds());
                }
                catch (Exception e)
                {
                    ErrorRegister = (true, e);
                    Finished = true;
                    return;
                }
            }

            _logger.LogCritical($"STOP THE TIMERS! Backtest took {(DateTimeOffset.Now - start).TotalMilliseconds}ms");
            _logger.LogCritical("Flushing the trades to the database...");
            _database.SaveChanges();
            _logger.LogCritical("Writing output");

            // Output to database
            var outputLogger = new BacktestOutputLogger(_database, this, _outputFolder);
            outputLogger.Output();

            // Declare completion (hands over control back to CLI)
            Finished = true;
        }
    }
}