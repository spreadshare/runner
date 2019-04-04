using System;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class of a state of a algorithm.
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm.</typeparam>
    internal abstract class State<T>
        where T : AlgorithmConfiguration
    {
        /// <summary>
        /// Whether or not the timer has been triggered.
        /// </summary>
        public bool TimerTriggered = true;

        private TimerProvider _timerProvider;

        /// <summary>
        /// Gets the time at which the timer should be triggered.
        /// </summary>
        public DateTimeOffset EndTime { get; private set; }

        /// <summary>
        /// Gets the logger of the state.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets a link to the parent algorithm settings.
        /// </summary>
        protected T AlgorithmConfiguration { get; private set; }

        /// <summary>
        /// Gets a shorthand alias for the first trading pair in the algorithm configuration.
        /// </summary>
        protected TradingPair FirstPair => AlgorithmConfiguration.TradingPairs.First();

        /// <summary>
        /// Gets a shorthand version for the CandleWidth in the algorithm configuration.
        /// </summary>
        protected int CandleWidth => AlgorithmConfiguration.CandleWidth;

        /// <summary>
        /// Initialise the state.
        /// </summary>
        /// <param name="algorithmConfiguration">Configuration of the algorithm.</param>
        /// <param name="container">Exchange service container.</param>
        /// <returns>The state that the Run method yields.</returns>
        public State<T> Activate(T algorithmConfiguration, ExchangeProvidersContainer container)
        {
            Logger = container.LoggerFactory.CreateLogger(GetType());
            AlgorithmConfiguration = algorithmConfiguration;
            _timerProvider = container.TimerProvider;
            return Run(container.TradingProvider, container.DataProvider);
        }

        /// <summary>
        /// Evaluates if the market condition is met.
        /// </summary>
        /// <param name="data">The data provider.</param>
        /// <returns>State to switch to.</returns>
        public virtual State<T> OnMarketCondition(DataProvider data) => new NothingState<T>();

        /// <summary>
        /// Evaluates if the order condition is met.
        /// </summary>
        /// <param name="order">The order update.</param>
        /// <returns>State to switch to.</returns>
        public virtual State<T> OnOrderUpdate(OrderUpdate order)
        {
            if (order == null || order.OrderType == OrderUpdate.OrderTypes.Market)
            {
                return new NothingState<T>();
            }

            if (Program.CommandLineArgs.Backtesting)
            {
                throw new AlgorithmLogicException($"Got order update in state {GetType().Name}, but the method was not implemented.");
            }

            Logger.LogError($"Got order update in state {GetType().Name}, but the method was not implemented.\n{JsonConvert.SerializeObject(order)}");
            return new NothingState<T>();
        }

        /// <summary>
        /// Evaluates when (if) a timer elapses.
        /// </summary>
        /// <returns>State to switch to.</returns>
        public virtual State<T> OnTimerElapsed()
        {
            if (Program.CommandLineArgs.Backtesting)
            {
                throw new AlgorithmLogicException($"Timer was triggered in state {GetType().Name}, but the method was not implemented.");
            }

            Logger.LogError($"Timer was triggered in state {GetType().Name}, but the method was not implemented.");
            return new NothingState<T>();
        }

        /// <summary>
        /// Set the timer for a certain timespan.
        /// </summary>
        /// <param name="duration">Duration to wait.</param>
        protected void SetTimer(TimeSpan duration)
        {
            Guard.Argument(duration.TotalMilliseconds).NotNegative();
            EndTime = _timerProvider.CurrentTime + duration;
            TimerTriggered = false;
        }

        /// <summary>
        /// Stalls the program until the start of a new candle.
        /// </summary>
        protected void WaitForNextCandle()
            => _timerProvider.WaitForNextCandle();

        /// <summary>
        /// Sets the post condition of a state.
        /// </summary>
        /// <param name="trading">Trading Provider.</param>
        /// <param name="data">Data provider.</param>
        /// <returns>State to switch to.</returns>
        protected virtual State<T> Run(TradingProvider trading, DataProvider data)
        {
            return new NothingState<T>();
        }
    }
}
