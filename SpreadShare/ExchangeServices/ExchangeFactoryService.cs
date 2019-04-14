using System;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Creates containers with Data-, Timer- and TradingProviders.
    /// </summary>
    internal class ExchangeFactoryService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly BinanceCommunicationsService _binanceCommunications;
        private readonly DatabaseContext _databaseContext;
        private readonly AllocationManager _allocationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging.</param>
        /// <param name="context">Injected database context.</param>
        /// <param name="alloc">Injected AllocationManager service.</param>
        /// <param name="binanceComm">Injected binance communication service.</param>
        public ExchangeFactoryService(
            ILoggerFactory loggerFactory,
            DatabaseContext context,
            AllocationManager alloc,
            BinanceCommunicationsService binanceComm)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
            _loggerFactory = loggerFactory;
            _databaseContext = context;
            _binanceCommunications = binanceComm;
            _allocationManager = alloc;
        }

        /// <summary>
        /// Builds container for Binance.
        /// </summary>
        /// <param name="algorithmConfiguration">The configuration of the algorithm.</param>
        /// <typeparam name="T">The type of the algorithm.</typeparam>
        /// <returns>Binance container with providers.</returns>
        public ExchangeProvidersContainer BuildContainer<T>(AlgorithmConfiguration algorithmConfiguration)
            where T : IBaseAlgorithm
        {
            if (!Reflections.AlgorithmMatchesConfiguration(typeof(T), algorithmConfiguration.GetType()))
            {
                throw new InvalidOperationException(
                    $"Cannot build container for {typeof(T).Name} using a {algorithmConfiguration.GetType().Name} object");
            }

            switch (Configuration.Instance.EnabledAlgorithm.Exchange)
            {
                case Exchange.Binance:
                    return BuildBinanceContainer<T>(algorithmConfiguration, _allocationManager);

                case Exchange.Backtesting:
                    return BuildBacktestingContainer<T>(algorithmConfiguration, _allocationManager);

                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmConfiguration));
            }
        }

        private ExchangeProvidersContainer BuildBinanceContainer<T>(AlgorithmConfiguration settings, AllocationManager allocationManager)
            where T : IBaseAlgorithm
        {
            // Makes sure that the communication is enabled
            _binanceCommunications.EnableStreams();
            var timerProvider = new BinanceTimerProvider(_loggerFactory, _binanceCommunications);
            var dataImplementation = new BinanceDataProvider(_loggerFactory, _binanceCommunications, timerProvider);
            var tradingImplementation = new BinanceTradingProvider(_loggerFactory, _binanceCommunications, timerProvider);

            var dataProvider = new DataProvider(_loggerFactory, dataImplementation, settings);
            var tradingProvider = new TradingProvider(_loggerFactory, tradingImplementation, dataProvider, allocationManager);

            // Doubly linked data provider to check candles
            timerProvider.DataProvider = dataProvider;

            // Inject database event listener
            DatabaseEventListenerService.Instance?.AddOrderSource(tradingProvider);

            return new ExchangeProvidersContainer(_loggerFactory, dataProvider, timerProvider, tradingProvider, typeof(T));
        }

        private ExchangeProvidersContainer BuildBacktestingContainer<T>(AlgorithmConfiguration settings, AllocationManager allocationManager)
            where T : IBaseAlgorithm
        {
            var backtestTimer = new BacktestTimerProvider(_loggerFactory, Configuration.Instance.BacktestSettings);
            var dataImplementation = new BacktestDataProvider(_loggerFactory, _databaseContext, backtestTimer);
            var tradingImplementation = new BacktestTradingProvider(_loggerFactory, backtestTimer, dataImplementation);

            var dataProvider = new DataProvider(_loggerFactory, dataImplementation, settings);
            var tradingProvider = new TradingProvider(_loggerFactory, tradingImplementation, dataProvider, allocationManager);

            // Doubly linked inheritance for backtesting edge case
            dataImplementation.ParentImplementation = dataProvider;
            tradingImplementation.ParentImplementation = tradingProvider;

            return new ExchangeProvidersContainer(_loggerFactory, dataProvider, backtestTimer, tradingProvider, typeof(T));
        }
    }
}
