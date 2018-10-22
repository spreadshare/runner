using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Binance;
using SpreadShare.ExchangeServices.Provider;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Creates containers with Data-, Timer- and TradingProviders
    /// </summary>
    internal class ExchangeFactoryService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        public ExchangeFactoryService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Builds container for Binance
        /// </summary>
        /// <param name="exchange">Specifies which container to create</param>
        /// <param name="allocationManager">Provides portfolio access</param>
        /// <returns>Binance container with providers</returns>
        public ExchangeProvidersContainer BuildContainer(Exchange exchange, WeakAllocationManager allocationManager)
        {
            AbstractDataProvider dataProviderImplementation;
            AbstractTradingProvider tradingProviderImplementation;
            switch (exchange)
            {
                case Exchange.Binance:
                    dataProviderImplementation = new BinanceDataProvider(_loggerFactory);
                    tradingProviderImplementation = new BinanceTradingProvider(_loggerFactory);
                    break;
                case Exchange.Backtesting:
                    throw new ArgumentOutOfRangeException(nameof(exchange), exchange, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(exchange), exchange, null);
            }

            return new ExchangeProvidersContainer(
                new DataProvider(dataProviderImplementation),
                new ExchangeTimerProvider(),
                new TradingProvider(tradingProviderImplementation, allocationManager));
        }
    }
}
