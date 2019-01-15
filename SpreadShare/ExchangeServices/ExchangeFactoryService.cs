using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Creates containers with Data-, Timer- and TradingProviders.
    /// </summary>
    internal class ExchangeFactoryService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SettingsService _settingsService;
        private readonly BinanceCommunicationsService _binanceCommunications;
        private readonly BacktestCommunicationService _backtestCommunicationService;
        private readonly DatabaseContext _databaseContext;
        private readonly AllocationManager _allocationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeFactoryService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging.</param>
        /// <param name="context">Injected database context.</param>
        /// <param name="alloc">Injected AllocationManager service.</param>
        /// <param name="settingsService">Injected settings.</param>
        /// <param name="binanceComm">Injected binance communication service.</param>
        /// <param name="backtestCom">Injected backtest communication service.</param>
        public ExchangeFactoryService(
            ILoggerFactory loggerFactory,
            DatabaseContext context,
            AllocationManager alloc,
            SettingsService settingsService,
            BinanceCommunicationsService binanceComm,
            BacktestCommunicationService backtestCom)
        {
            _logger = loggerFactory.CreateLogger<ExchangeFactoryService>();
            _loggerFactory = loggerFactory;

            _databaseContext = context;
            _settingsService = settingsService;

            // Link communication services
            _binanceCommunications = binanceComm;
            _backtestCommunicationService = backtestCom;

            // TODO: Reflections magic
            foreach (var item in settingsService.AllocationSettings)
            {
                switch (item.Key)
                {
                    case Exchange.Binance:
                        _binanceCommunications.Connect();
                        break;
                    case Exchange.Backtesting:
                        _backtestCommunicationService.Connect();
                        break;
                    default:
                        throw new MissingFieldException($"No communications instance for {item} in ExchangeFactory");
                }
            }

            _allocationManager = alloc;
        }

        /// <summary>
        /// Builds container for Binance.
        /// </summary>
        /// <param name="algorithm">The type of the algorithm.</param>
        /// <returns>Binance container with providers.</returns>
        public ExchangeProvidersContainer BuildContainer(Type algorithm)
        {
            var algorithmSettings = _settingsService.GetAlgorithSettings(algorithm);
            var allocationManager = _allocationManager.GetWeakAllocationManager(algorithm, algorithmSettings.Exchange);

            switch (algorithmSettings.Exchange)
            {
                case Exchange.Binance:
                    return BuildBinanceContainer(algorithmSettings, allocationManager, algorithm);

                case Exchange.Backtesting:
                    return BuildBacktestingContainer(algorithmSettings, allocationManager, algorithm);

                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm));
            }
        }

        private ExchangeProvidersContainer BuildBinanceContainer(AlgorithmSettings settings, WeakAllocationManager allocationManager, Type algorithm)
        {
            // Makes sure that the communication is enabled
            _binanceCommunications.Connect();
            var timerProvider = new ExchangeTimerProvider();
            var dataImplementation = new BinanceDataProvider(_loggerFactory, _binanceCommunications);
            var tradingImplementation = new BinanceTradingProvider(_loggerFactory, _binanceCommunications, timerProvider);

            var dataProvider = new DataProvider(_loggerFactory, dataImplementation, settings);
            var tradingProvider = new TradingProvider(_loggerFactory, tradingImplementation, dataProvider, allocationManager);
            return new ExchangeProvidersContainer(_loggerFactory, dataProvider, timerProvider, tradingProvider, algorithm);
        }

        private ExchangeProvidersContainer BuildBacktestingContainer(AlgorithmSettings settings, WeakAllocationManager allocationManager, Type algorithm)
        {
            _backtestCommunicationService.Connect();

            var backtestTimer = new BacktestTimerProvider(_loggerFactory, _databaseContext, _settingsService.BackTestSettings);
            var dataImplementation = new BacktestDataProvider(_loggerFactory, _databaseContext, backtestTimer, _backtestCommunicationService);
            var tradingImplementation = new BacktestTradingProvider(_loggerFactory, backtestTimer, dataImplementation, _backtestCommunicationService, _databaseContext);

            var dataProvider = new DataProvider(_loggerFactory, dataImplementation, settings);
            var tradingProvider = new TradingProvider(_loggerFactory, tradingImplementation, dataProvider, allocationManager);

            // Doubly linked inheritance for backtesting edge case
            dataImplementation.ParentImplementation = dataProvider;

            return new ExchangeProvidersContainer(_loggerFactory, dataProvider, backtestTimer, tradingProvider, algorithm);
        }
    }
}
