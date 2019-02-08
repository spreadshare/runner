using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices
{
    public class DataProviderUtilitiesTest
    {
        private readonly Random _random;

        public DataProviderUtilitiesTest()
        {
            _random = new Random(69420);
        }

        [Fact]
        public void CompressCandlesHappyFlow()
        {
            var input = GenerateCandles(10, 5);
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
            long startTime = input[0].Timestamp;
            foreach (var candle in output)
            {
                Assert.Equal(startTime, candle.Timestamp);
                startTime += 10L;
            }
        }

        [Fact]
        public void CompressCandlesSimpleCase()
        {
            var input = GenerateCandles(2, 5);
            decimal inputHigh = input.Max(x => x.High);
            decimal inputLow = input.Min(x => x.Low);
            decimal inputOpen = input[0].Open;
            decimal inputClose = input[input.Length - 1].Close;
            long beginTimestamp = 0;

            var output = DataProviderUtilities.CompressCandles(input, 2);
            Assert.Equal(2, input.Length);
            Assert.Single(output);
            Assert.Equal(inputHigh, output.Max(x => x.High));
            Assert.Equal(inputLow, output.Min(x => x.Low));
            Assert.Equal(inputOpen, output[0].Open);
            Assert.Equal(inputClose, output[output.Length - 1].Close);
            Assert.Equal(beginTimestamp, output[0].Timestamp);
        }

        [Fact]
        public void CompressHandlesHugeCase()
        {
            var input = GenerateCandles(1200, 5);
            var output = DataProviderUtilities.CompressCandles(input, 10);

            Assert.Equal(120, output.Length);

            long startTimeStamp = output[0].Timestamp;

            foreach (var candle in output)
            {
                Assert.Equal(startTimeStamp, candle.Timestamp);
                startTimeStamp += 50;
            }

            // Check that the volume is the aggregate of the parent candles
            for (int i = 0, j = 0; i < 1200; i += 10, j++)
            {
                var parents = input.Skip(i).Take(10);
                Assert.Equal(output[j].Volume, parents.Sum(x => x.Volume));
            }
        }

        [Fact]
        public void CompressCandlesIdentity()
        {
            var input = GenerateCandles(10, 5);
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
            var input = GenerateCandles(1, 5);
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
            var input = GenerateCandles(10, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => DataProviderUtilities.CompressCandles(input, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DataProviderUtilities.CompressCandles(input, -1));
        }

        private BacktestingCandle[] GenerateCandles(int count, int minutesWide, string pair = "EOSETH")
        {
            var result = new BacktestingCandle[count];
            int time = 0;
            for (int i = 0; i < count; i++)
            {
               result[i] = GenerateCandle(time, pair);
               time += minutesWide;
            }

            return result;
        }

        private BacktestingCandle GenerateCandle(int timestamp, string pair = "EOSETH")
        {
            decimal open = (decimal)_random.NextDouble() * 10;
            decimal close = (decimal)_random.NextDouble() * 10;
            decimal high = open + ((decimal)_random.NextDouble() * 5);
            decimal low = open - ((decimal)_random.NextDouble() * 5);
            decimal volume = (decimal)_random.NextDouble() * 5;
            return new BacktestingCandle(
                timestamp: timestamp,
                open: open,
                close: close,
                high: high,
                low: low,
                volume: volume,
                tradingPair: pair);
        }
    }
}