using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Binance;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Creates containers with Data-, Timer- and TradingProviders
    /// </summary>
    internal class ExchangeFactoryService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        public ExchangeFactoryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
        }

        /// <summary>
        /// Builds container for Binance
        /// </summary>
        /// <returns>Binance container with providers</returns>
        public ExchangeProvidersContainer BuildContainer()
        {
            var dataProviderImplementation = new BinanceDataProvider();
            var tradingProviderImplementation = new BinanceTradingProvider();

            return new ExchangeProvidersContainer(
                new DataProvider(dataProviderImplementation),
                new ExchangeTimerProvider(),
                new TradingProvider(tradingProviderImplementation));
        }
    }
}
