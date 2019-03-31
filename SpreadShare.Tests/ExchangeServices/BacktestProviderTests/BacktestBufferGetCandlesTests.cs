using System;
using System.Linq;
using System.Reflection;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models.Database;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BacktestProviderTests
{
    public class BacktestBufferGetCandlesTests : BaseTest
    {
        private Func<BacktestingCandle[], int, BacktestingCandle[]> _buildCandleBuffer;

        private BacktestingCandle[] _candles =
            {
                // #1
                new BacktestingCandle(
                    timestamp: 300000L,
                    open: 5,
                    close: 6.6M,
                    high: 7.2M,
                    low: 4.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #2
                new BacktestingCandle(
                    timestamp: 600000L,
                    open: 6.12M,
                    close: 8.01M,
                    high: 8.02M,
                    low: 6.0M,
                    volume: 3424,
                    tradingPair: "EOSETH"),

                // #3
                new BacktestingCandle(
                    timestamp: 900000L,
                    open: 7.90M,
                    close: 8.872M,
                    high: 8.9M,
                    low: 7.90M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #4
                new BacktestingCandle(
                    timestamp: 1200000L,
                    open: 7.6M,
                    close: 6.8M,
                    high: 7.8M,
                    low: 6.8M,
                    volume: 20453,
                    tradingPair: "EOSETH"),

                // #5
                new BacktestingCandle(
                    timestamp: 1500000L,
                    open: 7.9M,
                    close: 5.6M,
                    high: 7.9M,
                    low: 5.6M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #6
                new BacktestingCandle(
                    timestamp: 1800000L,
                    open: 5.9M,
                    close: 6.3M,
                    high: 6.6M,
                    low: 5.3M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #7
                new BacktestingCandle(
                    timestamp: 2100000L,
                    open: 6.4M,
                    close: 6.6M,
                    high: 7.2M,
                    low: 6.4M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #8
                new BacktestingCandle(
                    timestamp: 2400000L,
                    open: 6.5M,
                    close: 6.9M,
                    high: 7.4M,
                    low: 6.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #9
                new BacktestingCandle(
                    timestamp: 2700000L,
                    open: 6.7M,
                    close: 6.2M,
                    high: 6.8M,
                    low: 5.9M,
                    volume: 68453,
                    tradingPair: "EOSETH"),

                // #10
                new BacktestingCandle(
                    timestamp: 30000000L,
                    open: 6.2M,
                    close: 5.6M,
                    high: 6.4M,
                    low: 5.5M,
                    volume: 4053,
                    tradingPair: "EOSETH"),

                // #11
                new BacktestingCandle(
                    timestamp: 33000000L,
                    open: 5.6M,
                    close: 5.7M,
                    high: 5.8M,
                    low: 5.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),
            };

        public BacktestBufferGetCandlesTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var method = typeof(BacktestBuffers)
                .GetMethod("BuildCandleBuffer", BindingFlags.Static | BindingFlags.NonPublic);
            _buildCandleBuffer = (candles, channelWidth)
                =>
            {
                try
                {
                    return (BacktestingCandle[])method.Invoke(null, new object[] { candles, channelWidth });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            };
        }

        [Fact]
        public void BuildCandleBufferNotSaturated()
        {
            var candles = _candles.ToArray();
            var result = _buildCandleBuffer(candles, 10);
            Assert.Equal(5, result.Length);
        }

        [Fact]
        public void BuildCandleBufferIdentity()
        {
            var result = _buildCandleBuffer(_candles, 5);
            Assert.Equal(11, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.Equal(_candles[i].Low, result[i].Low);
                Assert.Equal(_candles[i].High, result[i].High);
                Assert.Equal(_candles[i].Open, result[i].Open);
                Assert.Equal(_candles[i].Close, result[i].Close);
                Assert.Equal(_candles[i].Timestamp, result[i].Timestamp);
                Assert.Equal(_candles[i].TradingPair, result[i].TradingPair);
            }
        }

        [Fact]
        public void BuildCandleBufferSingleCandleResult()
        {
            var candles = _candles.Skip(1).ToArray();
            var result = _buildCandleBuffer(candles, 30);
            Assert.Single(result);
            Assert.Equal(candles.Max(x => x.High), result[0].High);
            Assert.Equal(candles.Min(x => x.Low), result[0].Low);
        }

        [Fact]
        public void BuildCandleBufferInvalidCandleSize()
        {
            Assert.Throws<InvalidOperationException>(
                () => _buildCandleBuffer(_candles, 7));
        }

        [Fact]
        public void BuildCandleBufferSmallerCandleSize()
        {
            Assert.Throws<InvalidOperationException>(
                () => _buildCandleBuffer(_candles, 1));
        }
    }
}