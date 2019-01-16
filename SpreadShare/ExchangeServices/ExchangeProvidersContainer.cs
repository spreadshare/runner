using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Container that provides algorithms with the data gathering, timers and trading capabilities.
    /// </summary>
    internal class ExchangeProvidersContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeProvidersContainer"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging capabilities.</param>
        /// <param name="dataProvider">Provides data gathering capabilities.</param>
        /// <param name="timerProvider">Provides timer and scheduling capabilities.</param>
        /// <param name="tradingProvider">Provides trading capabilities.</param>
        /// <param name="algorithm">The algorithm to represent.</param>
        public ExchangeProvidersContainer(
            ILoggerFactory loggerFactory,
            DataProvider dataProvider,
            TimerProvider timerProvider,
            TradingProvider tradingProvider,
            Type algorithm)
        {
            LoggerFactory = loggerFactory;
            DataProvider = dataProvider;
            TimerProvider = timerProvider;
            TradingProvider = tradingProvider;
            Algorithm = algorithm;
        }

        /// <summary>
        /// Gets the provider for logging capabilities.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets the provider for data gathering capabilities.
        /// </summary>
        public DataProvider DataProvider { get; }

        /// <summary>
        /// Gets the provider for timer and scheduling capabilities.
        /// </summary>
        public TimerProvider TimerProvider { get; }

        /// <summary>
        /// Gets the provider for trading capabilities.
        /// </summary>
        public TradingProvider TradingProvider { get; }

        /// <summary>
        /// Gets the type of the algorithm this container represents.
        /// </summary>
        public Type Algorithm { get; }
    }
}
