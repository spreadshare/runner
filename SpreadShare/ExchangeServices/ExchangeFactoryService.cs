using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Binance;
using SpreadShare.ExchangeServices.ExchangeCommunicationService;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Creates containers with Data-, Timer- and TradingProviders
    /// </summary>
    internal class ExchangeFactoryService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly BinanceCommunicationsService _binanceCommunications;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        /// <param name="binanceComm">Injected binance communication service</param>
        public ExchangeFactoryService(ILoggerFactory loggerFactory, BinanceCommunicationsService binanceComm)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
            _loggerFactory = loggerFactory;

            // link communication services
            _binanceCommunications = binanceComm;
        }

        /// <summary>
        /// Start the factory service, which intern starts all the communication services.
        /// </summary>
        /// <returns>Response object indicating success or not</returns>
        public ResponseObject Start()
        {
            ResponseObject response;
            _logger.LogInformation("Starting binance communication service...");
            response = _binanceCommunications.Start();
            if (!response.Success)
            {
                _logger.LogError(response.ToString());
                return new ResponseObject(ResponseCode.Error, "Binance communications failed to start");
            }

            _logger.LogInformation("Binance communication successfully started");
            return new ResponseObject(ResponseCode.Success);
        }

        /// <summary>
        /// Builds container for Binance
        /// </summary>
        /// <returns>Binance container with providers</returns>
        public ExchangeProvidersContainer BuildContainer()
        {
            var dataProviderImplementation = new BinanceDataProvider(_loggerFactory, _binanceCommunications);
            var tradingProviderImplementation = new BinanceTradingProvider(_loggerFactory, _binanceCommunications);

            return new ExchangeProvidersContainer(
                new DataProvider(dataProviderImplementation),
                new ExchangeTimerProvider(),
                new TradingProvider(tradingProviderImplementation));
        }
    }
}
