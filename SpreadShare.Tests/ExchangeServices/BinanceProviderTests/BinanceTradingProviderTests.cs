using System;
using System.Reflection;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.Stubs.Binance;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    public class BinanceTradingProviderTests : BaseProviderTests
    {
        public BinanceTradingProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact(Skip = "Requires trading keys")]
        public void ExecuteMarketOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10);
        }

        [Fact(Skip = "Requires trading keys")]
        public void ExecuteMarketOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.ExecuteMarketOrderSell(TradingPair.Parse("EOSETH"), 10);
        }

        [Fact(Skip = "Requires trading keys")]
        public void PlaceLimitOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact(Skip = "Requires trading keys")]
        public void PlaceLimitOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact(Skip = "Requires trading keys")]
        public void PlaceStoplossOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact(Skip = "Requires trading keys")]
        public void PlaceStoplossOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), 100, 0.01M);
        }

        /// <summary>
        /// Creates a TradingProvider containing the Binance implementation, treated with
        /// a mock communications service.
        /// </summary>
        /// <returns>Testable trading provider.</returns>
        private TradingProvider GetTradingProvider()
        {
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            var trading = container.TradingProvider;
            var property = trading.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(TradingProvider)}");

            // Inject test communications
            var comms = new TestBinanceCommunicationsService(LoggerFactory);

            // Inject test implementation
            var binance = new BinanceTradingProvider(LoggerFactory, comms, container.TimerProvider);
            property.SetValue(trading, binance);
            return trading;
        }
    }
}