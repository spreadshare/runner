using Microsoft.Extensions.Logging;
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
        /// <param name="trading">Trading Provider</param>
        /// <param name="data">Data Provider</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public void Activate(T settings, TradingProvider trading, DataProvider data, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            AlgorithmSettings = settings;
            Run(trading, data);
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
        /// Validates if all the required parameters exist within the context
        /// </summary>
        /// <param name="trading">Trading Provider</param>
        /// <param name="data">Data provider</param>
        protected abstract void Run(TradingProvider trading, DataProvider data);
    }
}
