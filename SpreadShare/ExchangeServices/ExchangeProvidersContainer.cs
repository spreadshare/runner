using SpreadShare.ExchangeServices.Provider;

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
        /// <param name="dataProvider">Provides data gathering capabilities</param>
        /// <param name="timerProvider">Provides timer and scheduling capabilities</param>
        /// <param name="tradingProvider">Provides trading capabilities</param>
        public ExchangeProvidersContainer(IDataProvider dataProvider, ITimerProvider timerProvider, ITradingProvider tradingProvider)
        {
            DataProvider = dataProvider;
            TimerProvider = timerProvider;
            TradingProvider = tradingProvider;
        }

        /// <summary>
        /// Gets the provider for data gathering capabilities.
        /// </summary>
        public IDataProvider DataProvider { get; }

        /// <summary>
        /// Gets the provider for timer and scheduling capabilities.
        /// </summary>
        public ITimerProvider TimerProvider { get; }

        /// <summary>
        /// Gets the provider for trading capabilities.
        /// </summary>
        public ITradingProvider TradingProvider { get; }
    }
}
