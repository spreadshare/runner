using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceTests
{
    /// <summary>
    /// Tests for binance data provider.
    /// </summary>
    public class BinanceDataProviderTests : BaseProviderTests
    {
        private ExchangeProvidersContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceDataProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public BinanceDataProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _container = ExchangeFactoryService.BuildContainer(typeof(TemplateAlgorithm));
        }

        /// <summary>
        /// Check if the last trade price is a non zero decimal.
        /// </summary>
        /// <param name="input">String representation of the pair.</param>
        [Theory]
        [InlineData("BNBETH")]
        [InlineData("VETETH")]
        public void PriceLastTradeNonZero(string input)
        {
            var pair = TradingPair.Parse(input);
            var data = _container.DataProvider;
            var query = data.GetCurrentPriceTopAsk(pair);

            Assert.True(query > 0, $"{query} is not a valid price ({pair})");
        }

        /// <summary>
        /// Check if the top bid is lower than lowest ask.
        /// </summary>
        /// <param name="input">String representation of the pair.</param>
        [Theory]
        [InlineData("XRPBTC")]
        [InlineData("NEOBNB")]
        public void LowestAskHigherThanHighestBid(string input)
        {
            var pair = TradingPair.Parse(input);
            var data = _container.DataProvider;
            var topAsk = data.GetCurrentPriceTopAsk(pair);
            var topBid = data.GetCurrentPriceTopBid(pair);

            Assert.True(topAsk > topBid, $"Top bid is higher than lowest ask (bid: {topBid}, ask: {topAsk}");
        }
    }
}