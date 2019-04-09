using System;
using System.Linq;
using System.Reflection;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models.Database;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BacktestProviderTests
{
    public class BacktestBufferLowestLowTests : BaseTest
    {
        private Func<BacktestingCandle[], int, decimal[]> _buildLowestLowBuffer;

        private BacktestingCandle[] _candles =
            {
                // #1
                new BacktestingCandle(
                    openTimestamp: 300000L,
                    open: 5,
                    close: 6.6M,
                    high: 7.2M,
                    low: 4.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #2
                new BacktestingCandle(
                    openTimestamp: 600000L,
                    open: 6.12M,
                    close: 8.01M,
                    high: 8.02M,
                    low: 6.0M,
                    volume: 3424,
                    tradingPair: "EOSETH"),

                // #3
                new BacktestingCandle(
                    openTimestamp: 900000L,
                    open: 7.90M,
                    close: 8.872M,
                    high: 8.9M,
                    low: 7.90M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #4
                new BacktestingCandle(
                    openTimestamp: 1200000L,
                    open: 7.6M,
                    close: 6.8M,
                    high: 7.8M,
                    low: 6.8M,
                    volume: 20453,
                    tradingPair: "EOSETH"),

                // #5
                new BacktestingCandle(
                    openTimestamp: 1500000L,
                    open: 7.9M,
                    close: 5.6M,
                    high: 7.9M,
                    low: 5.6M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #6
                new BacktestingCandle(
                    openTimestamp: 1800000L,
                    open: 5.9M,
                    close: 6.3M,
                    high: 6.6M,
                    low: 5.3M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #7
                new BacktestingCandle(
                    openTimestamp: 2100000L,
                    open: 6.4M,
                    close: 6.6M,
                    high: 7.2M,
                    low: 6.4M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #8
                new BacktestingCandle(
                    openTimestamp: 2400000L,
                    open: 6.5M,
                    close: 6.9M,
                    high: 7.4M,
                    low: 6.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #9
                new BacktestingCandle(
                    openTimestamp: 2700000L,
                    open: 6.7M,
                    close: 6.2M,
                    high: 6.8M,
                    low: 5.9M,
                    volume: 68453,
                    tradingPair: "EOSETH"),

                // #10
                new BacktestingCandle(
                    openTimestamp: 30000000L,
                    open: 6.2M,
                    close: 5.6M,
                    high: 6.4M,
                    low: 5.5M,
                    volume: 4053,
                    tradingPair: "EOSETH"),

                // #11
                new BacktestingCandle(
                    openTimestamp: 33000000L,
                    open: 5.6M,
                    close: 5.7M,
                    high: 5.8M,
                    low: 5.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),
            };

        public BacktestBufferLowestLowTests(ITestOutputHelper output)
            : base(output)
        {
            var method = typeof(BacktestBuffers)
                .GetMethod("BuildLowestLowBuffer", BindingFlags.NonPublic | BindingFlags.Static);
            _buildLowestLowBuffer = (input, channelWidth) =>
                (decimal[])method.Invoke(null, new object[] { input, channelWidth });
        }

        [Fact]
        public void LowestLowSingularCandles()
        {
            var result = _buildLowestLowBuffer(_candles, 1);
            Assert.Equal(_candles.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.Equal(_candles[i].Low, result[i]);
            }
        }

        [Fact]
        public void LowestLowDualCandles()
        {
            var data = _candles;
            var result = _buildLowestLowBuffer(data, 2);
            Assert.Equal(data.Length, result.Length);
            for (int i = 1; i < result.Length; i++)
            {
                Assert.Equal(new[] { data[i - 1], data[i] }.Min(x => x.Low), result[i]);
            }
        }

        [Fact]
        public void LowestLowAllCandles()
        {
            var data = _candles;
            var result = _buildLowestLowBuffer(data, data.Length);
            Assert.Equal(data.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
               Assert.Equal(data.Take(i + 1).Min(x => x.Low), result[i]);
            }
        }

        [Fact]
        public void LowestLowHalfSize()
        {
            var data = _candles;
            var result = _buildLowestLowBuffer(data, 5);
            Assert.Equal(data.Length, result.Length);
            for (int i = 4; i < result.Length; i++)
            {
                Assert.Equal(_candles.Skip(i - 4).Take(5).Min(x => x.Low), result[i]);
            }
        }

        [Fact]
        public void LowestLowOneCandle()
        {
            var input = new BacktestingCandle[]
            {
                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 15M,
                    high: 20M,
                    low: 5,
                    volume: 100,
                    tradingPair: "EOSETH"),
            };

            var result = _buildLowestLowBuffer(input, 1);
            Assert.Single(result);
            Assert.Equal(5M, result[0]);
        }

        [Fact]
        public void LowestLowTwoCandles()
        {
            var input = new BacktestingCandle[]
            {
                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 15M,
                    high: 20M,
                    low: 5,
                    volume: 100,
                    tradingPair: "EOSETH"),

                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 12M,
                    high: 14M,
                    low: 3,
                    volume: 100,
                    tradingPair: "EOSETH"),
            };

            var result = _buildLowestLowBuffer(input, 1);
            Assert.Equal(2, result.Length);
            Assert.Equal(5M, result[0]);
            Assert.Equal(3M, result[1]);
        }

        [Fact]
        public void LowestLowTwoCandlesTogetherLarger()
        {
            var input = new BacktestingCandle[]
            {
                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 15M,
                    high: 20M,
                    low: 5,
                    volume: 100,
                    tradingPair: "EOSETH"),

                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 12M,
                    high: 21M,
                    low: 6,
                    volume: 100,
                    tradingPair: "EOSETH"),
            };

            var result = _buildLowestLowBuffer(input, 2);
            Assert.Equal(2, result.Length);
            Assert.Equal(5M, result[0]);
            Assert.Equal(5M, result[1]);
        }

        [Fact]
        public void LowestLowTwoCandlesTogetherSmaller()
        {
            var input = new BacktestingCandle[]
            {
                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 15M,
                    high: 20M,
                    low: 5,
                    volume: 100,
                    tradingPair: "EOSETH"),

                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 10M,
                    close: 12M,
                    high: 12M,
                    low: 8,
                    volume: 100,
                    tradingPair: "EOSETH"),
            };

            var result = _buildLowestLowBuffer(input, 2);
            Assert.Equal(2, result.Length);
            Assert.Equal(5, result[0]);
            Assert.Equal(5M, result[1]);
        }

        [Fact]
        public void LowestLowHugeCase()
        {
            var random = new Random();
            var candles = new BacktestingCandle[1451];

            for (int i = 0; i < candles.Length; i++)
            {
                var open = (decimal)(random.NextDouble() * 30) + 1;
                var close = (decimal)(random.NextDouble() * 30) + 1;
                var high = (decimal)(random.NextDouble() * 30) + 1;
                var low = (decimal)(random.NextDouble() * 30) + 1;
                var volume = (decimal)(random.NextDouble() * 420);

                candles[i] = new BacktestingCandle(
                    openTimestamp: i * 60000,
                    open: open,
                    close: close,
                    high: high,
                    low: low,
                    volume: volume,
                    tradingPair: "EOSETH");
            }

            var result = _buildLowestLowBuffer(candles, 400);

            for (int i = 399; i < candles.Length; i++)
            {
                Assert.Equal(candles.Skip(i - 399).Take(400).Min(x => x.Low), result[i]);
            }
        }
    }
}