using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using SpreadShare.Models.Database;

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
            int offset = 0;
            var result = new List<BacktestingCandle>();

            while (offset + compressionRatio <= input.Length)
            {
                var subset = input.Skip(offset).Take(compressionRatio).ToList();
                var first = subset[subset.Count - 1];
                var last = subset[0];
                result.Add(new BacktestingCandle(
                    timestamp: last.Timestamp,
                    open: first.Open,
                    close: last.Close,
                    high: subset.Max(x => x.High),
                    low: subset.Min(x => x.Low),
                    volume: subset.Sum(x => x.Volume),
                    tradingPair: first.TradingPair));
                offset += compressionRatio;
            }

            return result.ToArray();
        }
    }
}