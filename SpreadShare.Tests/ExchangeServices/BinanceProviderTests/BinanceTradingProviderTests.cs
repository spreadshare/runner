using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    public class BinanceTradingProviderTests : BinanceTestUtils
    {
        public BinanceTradingProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ExecuteMarketOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10);
        }

        [Fact]
        public void ExecuteMarketOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.ExecuteMarketOrderSell(TradingPair.Parse("EOSETH"), 10);
        }

        [Fact]
        public void PlaceLimitOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact]
        public void PlaceLimitOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact]
        public void PlaceStoplossOrderBuyHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10, 0.01M);
        }

        [Fact]
        public void PlaceStoplossOrderSellHappyFlow()
        {
            var trading = GetTradingProvider();
            trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), 100, 0.01M);
        }
    }
}