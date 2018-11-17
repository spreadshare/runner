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
    /// Creates containers with Data-, Timer- and TradingProviders
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
        /// <param name="loggerFactory">Provides logging</param>
        /// <param name="context">Injected database context</param>
        /// <param name="alloc">Injected AllocationManager service</param>
        /// <param name="settingsService">Injected settings</param>
        /// <param name="binanceComm">Injected binance communication service</param>
        /// <param name="backtestCom">Injected backtest communication service</param>
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

            _allocationManager = alloc;
        }

        /// <summary>
        /// Builds container for Binance
        /// </summary>
        /// <param name="exchange">Specifies which container to create</param>
        /// <param name="algorithm">The type of the algorithm</param>
        /// <returns>Binance container with providers</returns>
        public ExchangeProvidersContainer BuildContainer(
            Exchange exchange,
            Type algorithm)
        {
            AbstractDataProvider dataProviderImplementation;
            AbstractTradingProvider tradingProviderImplementation;
            TimerProvider timerProvider = new ExchangeTimerProvider();

            switch (exchange)
            {
                case Exchange.Binance:
                    dataProviderImplementation = new BinanceDataProvider(_loggerFactory, _binanceCommunications);
                    tradingProviderImplementation = new BinanceTradingProvider(_loggerFactory, _binanceCommunications);
                    break;

                case Exchange.Backtesting:
                    // Override timer provider to backtest variant
                    timerProvider = new BacktestTimerProvider(
                        _loggerFactory,
                        DateTimeOffset.FromUnixTimeMilliseconds(_settingsService.BackTestSettings.BeginTimeStamp),
                        DateTimeOffset.FromUnixTimeMilliseconds(_settingsService.BackTestSettings.EndTimeStamp));

                    dataProviderImplementation = new BacktestDataProvider(_loggerFactory, _databaseContext, (BacktestTimerProvider)timerProvider, _backtestCommunicationService);
                    tradingProviderImplementation = new BacktestTradingProvider(
                        _loggerFactory,
                        (BacktestTimerProvider)timerProvider,
                        (BacktestDataProvider)dataProviderImplementation,
                        _backtestCommunicationService);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(exchange), exchange, null);
            }

            var allocationManager = _allocationManager.GetWeakAllocationManager(algorithm, exchange);
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
