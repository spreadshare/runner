using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class CompressCandlesTests : CandleTest
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

        private BacktestingCandle[] GetFiveMinuteCandles(int count)
        {
            return Candles.Take(count).ToArray();
        }
    }
}