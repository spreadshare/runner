using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class CompressCandlesTests
    {
        [Fact]
        public void CompressCandlesHappyFlow()
        {
            var input = GetFiveMinuteCandles(10);
            decimal inputHigh = input.Max(x => x.High);
            decimal inputLow = input.Min(x => x.Low);
            decimal inputOpen = input[input.Length - 1].Open;
            decimal inputClose = input[0].Close;

            var output = DataProviderUtilities.CompressCandles(input, 2);
            Assert.Equal(10, input.Length);
            Assert.Equal(5, output.Length);
            Assert.Equal(inputHigh, output.Max(x => x.High));
            Assert.Equal(inputLow, output.Min(x => x.Low));
            Assert.Equal(inputOpen, output[output.Length - 1].Open);
            Assert.Equal(inputClose, output[0].Close);

            // Assert that the interval has doubled.
            long startTime = input[0].Timestamp;
            foreach (var candle in output)
            {
                Assert.Equal(startTime, candle.Timestamp);
                startTime -= 600000L;
            }
        }

        [Fact]
        public void CompressCandlesSimpleCase()
        {
            var input = GetFiveMinuteCandles(2);
            decimal inputHigh = input.Max(x => x.High);
            decimal inputLow = input.Min(x => x.Low);
            decimal inputOpen = input[input.Length - 1].Open;
            decimal inputClose = input[0].Close;
            long beginTimestamp = input[0].Timestamp;

            var output = DataProviderUtilities.CompressCandles(input, 2);
            Assert.Equal(2, input.Length);
            Assert.Single(output);
            Assert.Equal(inputHigh, output[0].High);
            Assert.Equal(inputLow, output[0].Low);
            Assert.Equal(inputOpen, output[0].Open);
            Assert.Equal(inputClose, output[output.Length - 1].Close);
            Assert.Equal(beginTimestamp, output[0].Timestamp);
        }

        [Fact]
        public void CompressCandlesIdentity()
        {
            var input = GetFiveMinuteCandles(10);
            decimal inputHigh = input.Max(x => x.High);
            decimal inputLow = input.Min(x => x.Low);
            decimal inputOpen = input[0].Open;
            decimal inputClose = input[input.Length - 1].Close;

            var output = DataProviderUtilities.CompressCandles(input, 1);
            Assert.Equal(10, input.Length);
            Assert.Equal(10, output.Length);
            Assert.Equal(inputHigh, output.Max(x => x.High));
            Assert.Equal(inputLow, output.Min(x => x.Low));
            Assert.Equal(inputOpen, output[0].Open);
            Assert.Equal(inputClose, output[output.Length - 1].Close);
        }

        [Fact]
        public void CompressCandlesInputSizeNotWholeMultiple()
        {
            var input = GetFiveMinuteCandles(1);
            Assert.Throws<ArgumentException>(() => DataProviderUtilities.CompressCandles(input, 2));
        }

        [Fact]
        public void CompressCandlesNull()
        {
            Assert.Throws<ArgumentNullException>(() => DataProviderUtilities.CompressCandles(null, 2));
        }

        [Fact]
        public void CompressCandlesEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                DataProviderUtilities.CompressCandles(Array.Empty<BacktestingCandle>(), 1));
        }

        [Fact]
        public void CompressCandlesRatioZeroOrNegative()
        {
            var input = GetFiveMinuteCandles(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => DataProviderUtilities.CompressCandles(input, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DataProviderUtilities.CompressCandles(input, -1));
        }

        private static BacktestingCandle[] GetFiveMinuteCandles(int count)
        {
            // This array is reversed before it is returned.
            return new[]
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
                    timestamp: 3000000L,
                    open: 6.2M,
                    close: 5.6M,
                    high: 6.4M,
                    low: 5.5M,
                    volume: 4053,
                    tradingPair: "EOSETH"),

                // #11
                new BacktestingCandle(
                    timestamp: 3300000L,
                    open: 5.6M,
                    close: 5.7M,
                    high: 5.8M,
                    low: 5.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),
            }.Reverse().Take(count).ToArray();
        }
    }
}