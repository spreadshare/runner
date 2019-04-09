using System;
using CSharpx;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Tests.ExchangeServices.DataProviderTests;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    /// <summary>
    /// Tests for binance data provider.
    /// </summary>
    public class BinanceDataProviderTests : DataProviderTestUtils
    {
        private readonly ExchangeProvidersContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceDataProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public BinanceDataProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            string source = $@"
                TradingPairs: [EOSETH]
                CandleWidth: {Configuration.Instance.CandleWidth} 
            ";
            var config = ParseAlgorithmConfiguration(source);
            _container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(config);
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

        [Theory]
        [InlineData(1)]
        [InlineData(5)]

        // [InlineData(1200)]
        public void GetCandlesHappyFlow(int limit)
        {
            var candles = _container.DataProvider.GetCandles(
                TradingPair.Parse("EOSETH"),
                limit);
            Assert.Equal(limit, candles.Length);
            Assert.All(candles, x =>
            {
                var unused = x ?? throw new Exception("Some candles where null");
            });
        }

        [Theory]
        [InlineData(5)]
        [InlineData(12)]
        [InlineData(1200)]
        public void GetCandlesTimestampIncreasing(int limit)
        {
            var candles = _container.DataProvider.GetCandles(
                TradingPair.Parse("EOSETH"),
                limit);
            var increment = (long)TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth).TotalMilliseconds;
            var diffs = candles.Pairwise((a, b) => b.OpenTimestamp - a.OpenTimestamp);
            foreach (var diff in diffs)
            {
                Assert.Equal(increment, diff);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(1200)]
        public void RecentCandleIsNow(int limit)
        {
            var candles = _container.DataProvider.GetCandles(
                TradingPair.Parse("EOSETH"),
                limit);
            var lastCandle = candles[candles.Length - 1].OpenTimestamp + (Configuration.Instance.CandleWidth * 60 * 1000);
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Logger.LogCritical($"LastCandle: {DateTimeOffset.FromUnixTimeMilliseconds(lastCandle)} - Now: {DateTimeOffset.FromUnixTimeMilliseconds(now)}");
            var diff = now - lastCandle;
            Assert.False(
                diff < 0,
                $"Most recent candle was not yet closed, candle closes at {DateTimeOffset.FromUnixTimeMilliseconds(lastCandle)}, but it is only {DateTimeOffset.Now}");
            Assert.True(
                diff < Configuration.Instance.CandleWidth * 60 * 1000,
                $"Most recent candle was not within the scope of {Configuration.Instance.CandleWidth}m (expected: {DateTimeOffset.FromUnixTimeMilliseconds(lastCandle)}, got {DateTimeOffset.FromUnixTimeMilliseconds(now)}), diff {diff}");
        }
    }
}