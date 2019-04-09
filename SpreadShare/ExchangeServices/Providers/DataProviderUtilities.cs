using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using SpreadShare.Models.Database;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// A collection of pure utility methods used by the DataProvider.
    /// </summary>
    internal static class DataProviderUtilities
    {
        /// <summary>
        /// Compress a list of candles given a certain ratio.
        /// </summary>
        /// <param name="input">The collection of candles to compress.</param>
        /// <param name="compressionRatio">The reducing ratio. (e.g. 2 -> 10 candles become 5 candles.</param>
        /// <returns>An array of candles.</returns>
        public static BacktestingCandle[] CompressCandles(
            BacktestingCandle[] input,
            int compressionRatio)
        {
            Guard.Argument(compressionRatio)
                .NotZero()
                .NotNegative();

            Guard.Argument(input)
                .NotNull(nameof(input))
                .NotEmpty()
                .Require<ArgumentException>(
                    x => x.Length % compressionRatio == 0,
                    x => $"{nameof(x)} has length {x.Length} which is not divisible by {nameof(compressionRatio)}, namely {compressionRatio}");

            // number of candles used to create new candles
            var result = new BacktestingCandle[input.Length / compressionRatio];

            Parallel.For(0, result.Length, index =>
            {
                var subset = input.Skip(index * compressionRatio).Take(compressionRatio).ToArray();
                var first = subset[0];
                var last = subset[subset.Length - 1];

                result[index] = new BacktestingCandle(
                    openTimestamp: first.OpenTimestamp,
                    open: first.Open,
                    close: last.Close,
                    high: subset.Max(x => x.High),
                    low: subset.Min(x => x.Low),
                    volume: subset.Sum(x => x.Volume),
                    tradingPair: first.TradingPair);
            });

            return result;
        }

        /// <summary>
        /// Calculates the average true range over a set of candles.
        /// </summary>
        /// <param name="input">Set of candles.</param>
        /// <returns>AverageTrueRange value.</returns>
        /// <exception cref="InvalidOperationException">For an empty set.</exception>
        public static decimal AverageTrueRange(this IEnumerable<BacktestingCandle> input)
        {
            var candles = (input ?? throw new ArgumentNullException(nameof(input))).ToArray();
            if (candles.Length < 2)
            {
                throw new InvalidOperationException($"Cannot calculate the AverageTrueRange of a set containing {candles.Length} candles.");
            }

            var trueRanges = new decimal[candles.Length - 1];

            // Calculate maximum of three edge features over the series (edgeCandle -> chunk[0] -> chunk[1] ... -> chunk[n])
            for (int i = 1; i < candles.Length; i++)
            {
                decimal highLow = Math.Abs(candles[i].High - candles[i].Low);
                decimal highPreviousClose = Math.Abs(candles[i].High - candles[i - 1].Close);
                decimal lowPreviousClose = Math.Abs(candles[i].Low - candles[i - 1].Close);

                trueRanges[i - 1] = new[] { highLow, highPreviousClose, lowPreviousClose }.Max();
            }

            return trueRanges.Average();
        }

        /// <summary>
        /// Calculates the standard moving average over a set of candles.
        /// </summary>
        /// <param name="input">Set of candles.</param>
        /// <returns>StandardMovingAverage value.</returns>
        public static decimal StandardMovingAverage(this IEnumerable<BacktestingCandle> input)
        {
            var candles = (input ?? throw new ArgumentNullException(nameof(input))).ToArray();
            if (candles.Length == 0)
            {
                throw new InvalidOperationException($"Cannot calculate the StandardMovingAverage of an empty set.");
            }

            return candles.Average(x => x.Close);
        }

        /// <summary>
        /// Calculates the rate of change over a set of candles.
        /// </summary>
        /// <param name="input">Set of candles.</param>
        /// <returns>RateOfChange value.</returns>
        public static decimal RateOfChange(this IEnumerable<BacktestingCandle> input)
        {
            var candles = (input ?? throw new ArgumentNullException(nameof(input))).ToArray();
            if (candles.Length == 0)
            {
                throw new InvalidOperationException("Cannot calculate the RateOfChange of an empty set.");
            }

            var current = candles[0];
            var past = candles.Last();
            return HelperMethods.SafeDiv(current.Close - past.Close, past.Close);
        }
    }
}