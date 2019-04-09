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
            decimal inputOpen = input[0].Open;
            decimal inputClose = input[input.Length - 1].Close;

            var output = DataProviderUtilities.CompressCandles(input, 2);
            Assert.Equal(10, input.Length);
            Assert.Equal(5, output.Length);
            Assert.Equal(inputHigh, output.Max(x => x.High));
            Assert.Equal(inputLow, output.Min(x => x.Low));
            Assert.Equal(inputOpen, output[0].Open);
            Assert.Equal(inputClose, output[output.Length - 1].Close);

            // Assert that the interval has doubled.
            long openTime = input[0].OpenTimestamp;
            foreach (var candle in output)
            {
                Assert.Equal(openTime, candle.OpenTimestamp);
                openTime += 600000L;
            }
        }

        [Fact]
        public void CompressCandlesSimpleCase()
        {
            var input = GetFiveMinuteCandles(2);
            decimal inputHigh = input.Max(x => x.High);
            decimal inputLow = input.Min(x => x.Low);
            decimal inputOpen = input[0].Open;
            decimal inputClose = input[input.Length - 1].Close;
            long openTimestamp = input[0].OpenTimestamp;

            var output = DataProviderUtilities.CompressCandles(input, 2);
            Assert.Equal(2, input.Length);
            Assert.Single(output);
            Assert.Equal(inputHigh, output[0].High);
            Assert.Equal(inputLow, output[0].Low);
            Assert.Equal(inputOpen, output[0].Open);
            Assert.Equal(inputClose, output[0].Close);
            Assert.Equal(openTimestamp, output[0].OpenTimestamp);
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
                    openTimestamp: 3000000L,
                    open: 6.2M,
                    close: 5.6M,
                    high: 6.4M,
                    low: 5.5M,
                    volume: 4053,
                    tradingPair: "EOSETH"),

                // #11
                new BacktestingCandle(
                    openTimestamp: 3300000L,
                    open: 5.6M,
                    close: 5.7M,
                    high: 5.8M,
                    low: 5.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),
            }.Take(count).ToArray();
        }
    }
}