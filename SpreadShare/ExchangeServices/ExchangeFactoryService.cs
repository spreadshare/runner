using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models;
using SpreadShare.SupportServices;

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
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        /// <param name="context">Injected database context</param>
        /// <param name="binanceComm">Injected binance communication service</param>
        public ExchangeFactoryService(
            ILoggerFactory loggerFactory,
            DatabaseContext context,
            BinanceCommunicationsService binanceComm)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
            _loggerFactory = loggerFactory;

            _databaseContext = context;

            // link communication services
            _binanceCommunications = binanceComm;
        }

        /// <summary>
        /// Start the factory service, which intern starts all the communication services.
        /// </summary>
        /// <returns>Response object indicating success or not</returns>
        public ResponseObject Start()
        {
            _logger.LogInformation("Starting binance communication service...");
            var response = _binanceCommunications.Start();
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
        /// <param name="exchange">Specifies which container to create</param>
        /// <param name="algorithm">The type of the algorithm</param>
        /// <param name="allocationManager">Provides portfolio access</param>
        /// <returns>Binance container with providers</returns>
        public ExchangeProvidersContainer BuildContainer(
            Exchange exchange,
            Type algorithm,
            WeakAllocationManager allocationManager)
        {
            AbstractDataProvider dataProviderImplementation;
            AbstractTradingProvider tradingProviderImplementation;
            ITimerProvider timerProvider = new ExchangeTimerProvider();

            switch (exchange)
            {
                case Exchange.Binance:
                    dataProviderImplementation = new BinanceDataProvider(_loggerFactory, _binanceCommunications);
                    tradingProviderImplementation = new BinanceTradingProvider(_loggerFactory, _binanceCommunications);
                    break;

                case Exchange.Backtesting:
                    // Override timer provider to backtest variant
                    timerProvider = new BacktestTimerProvider(_loggerFactory, DateTimeOffset.Now);

                    dataProviderImplementation = new BacktestDataProvider(_loggerFactory, _databaseContext, timerProvider as BacktestTimerProvider);
                    tradingProviderImplementation = new BacktestTradingProvider(_loggerFactory, timerProvider as BacktestTimerProvider);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(exchange), exchange, null);
            }

            var dataProvider = new DataProvider(dataProviderImplementation);
            var tradingProvider = new TradingProvider(_loggerFactory, tradingProviderImplementation, dataProvider, allocationManager, algorithm, exchange);

            return new ExchangeProvidersContainer(
                _loggerFactory,
                dataProvider,
                timerProvider,
                tradingProvider);
        }
    }
}
