using System;
using Dawn;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class of a state of a algorithm
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm</typeparam>
    internal abstract class State<T>
        where T : AlgorithmSettings
    {
        /// <summary>
        /// Whether or not the timer has been triggered.
        /// </summary>
        public bool TimerTriggered = true;

        private TimerProvider _timerProvider;

        /// <summary>
        /// Gets the time at which the timer should be triggered
        /// </summary>
        public DateTimeOffset EndTime { get; private set; }

        /// <summary>
        /// Gets the logger of the state
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets a link to the parent algorithm settings
        /// </summary>
        protected T AlgorithmSettings { get; private set; }

        /// <summary>
        /// Initialise the state
        /// </summary>
        /// <param name="settings">Algorithm settings object</param>
        /// <param name="container">Exchange service container</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public void Activate(T settings, ExchangeProvidersContainer container, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            AlgorithmSettings = settings;
            _timerProvider = container.TimerProvider;
            Run(container.TradingProvider, container.DataProvider);
        }

        /// <summary>
        /// Evaluates if the market condition is met.
        /// </summary>
        /// <param name="data">The data provider</param>
        /// <returns>State to switch to</returns>
        public virtual State<T> OnMarketCondition(DataProvider data) => new NothingState<T>();

        /// <summary>
        /// Evaluates if the order condition is met.
        /// </summary>
        /// <param name="order">The order update</param>
        /// <returns>State to switch to</returns>
        public virtual State<T> OnOrderUpdate(OrderUpdate order) => new NothingState<T>();

        /// <summary>
        /// Evaluates when (if) a timer elapses.
        /// </summary>
        /// <returns>State to switch to</returns>
        public virtual State<T> OnTimerElapsed() => new NothingState<T>();

        /// <summary>
        /// Set the timer for a certain timespan
        /// </summary>
        /// <param name="duration">Duration to wait</param>
        protected void SetTimer(TimeSpan duration)
        {
            Guard.Argument(duration.TotalMilliseconds).NotNegative();
            EndTime = _timerProvider.CurrentTime + duration;
            TimerTriggered = false;
        }

        /// <summary>
        /// Sets the post condition of a state
        /// </summary>
        /// <param name="trading">Trading Provider</param>
        /// <param name="data">Data provider</param>
        protected abstract void Run(TradingProvider trading, DataProvider data);
    }
}
