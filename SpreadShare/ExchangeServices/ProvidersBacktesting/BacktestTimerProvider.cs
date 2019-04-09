using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
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
        private readonly List<BacktestOrder> _backtestOrders;

        private DateTimeOffset _currentTime;
        private DateTimeOffset _lastCandleOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="database">The database context for flushing.</param>
        /// <param name="settings">Provides startDate, endDate and outputFolder.</param>
        public BacktestTimerProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestSettings settings)
            : base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _database = database;
            _backtestOrders = new List<BacktestOrder>();

            // Hardcoded 2 week offset
            BeginTime = DateTimeOffset.FromUnixTimeMilliseconds(
                BacktestDaemonService.Instance.State.BeginTimeStamp) + TimeSpan.FromDays(14);
            _currentTime = BeginTime;
            _lastCandleOpen = _currentTime;

            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(
                BacktestDaemonService.Instance.State.EndTimeStamp);
            _outputFolder = settings.OutputFolder;
        }

        /// <summary>
        /// Gets the current time of the backtest universe.
        /// </summary>
        public override DateTimeOffset CurrentTime => _currentTime;

        /// <inheritdoc/>
        public override DateTimeOffset LastCandleOpen => _lastCandleOpen;

        /// <inheritdoc />
        public override DateTimeOffset Pivot => BeginTime;

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

        /// <summary>
        /// Add order to the logger.
        /// </summary>
        /// <param name="order">Order to log.</param>
        public void AddOrder(BacktestOrder order)
        {
            _backtestOrders.Add(order);
        }

        /// <inheritdoc />
        public override async void RunPeriodicTimer()
        {
            // Make sure all constructor processes are finished
            await Task.Delay(1000).ConfigureAwait(false);

            // Clear the trades and state switch event table (NOTE: Sequences are not reset)
            _database.Database.ExecuteSqlCommand("TRUNCATE TABLE public.\"StateSwitchEvents\";");
            _database.SaveChanges();
            _backtestOrders.Clear();

            while (CurrentTime < EndTime)
            {
                try
                {
                    _currentTime += TimeSpan.FromMinutes(Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration.CandleWidth);
                    _lastCandleOpen = _currentTime;
                    UpdateObservers(_currentTime.ToUnixTimeMilliseconds());
                }
                catch (Exception e)
                {
                    ErrorRegister = (true, e);
                    Finished = true;
                    return;
                }
            }

            LogOutput();
        }

        /// <inheritdoc />
        public override void WaitForNextCandle()
        {
            _currentTime += TimeSpan.FromMinutes(Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration.CandleWidth);
            _lastCandleOpen = _currentTime;
        }

        /// <summary>
        /// Stop the timer and log the results.
        /// </summary>
        public void LogOutput()
        {
            _logger.LogInformation("Writing output");

            // Output to database
            var outputLogger = new BacktestOutputLogger(_database, this, _outputFolder, _backtestOrders);
            outputLogger.Output();

            // Declare completion (hands over control back to CLI)
            Finished = true;
        }
    }
}