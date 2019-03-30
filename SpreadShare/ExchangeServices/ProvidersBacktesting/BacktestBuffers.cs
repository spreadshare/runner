using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Caches database backtest data in memory for rapid backtesting.
    /// </summary>
    internal class BacktestBuffers
    {
        private static Dictionary<string, BacktestingCandle[]> _buffers;
        private static Dictionary<(string, int), decimal[]> _highestHighBuffer;
        private static Dictionary<(string, int), decimal[]> _lowestLowBuffer;
        private static Dictionary<(string, int), BacktestingCandle[]> _candleBuffer;

        private readonly DatabaseContext _db;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestBuffers"/> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="logger">Logger to create output.</param>
        public BacktestBuffers(DatabaseContext db, ILogger logger)
        {
            _db = db;
            _logger = logger;

            if (_buffers == null)
            {
                _buffers = new Dictionary<string, BacktestingCandle[]>();
            }

            if (_highestHighBuffer == null)
            {
                _highestHighBuffer = new Dictionary<(string, int), decimal[]>();
            }

            if (_lowestLowBuffer == null)
            {
                _lowestLowBuffer = new Dictionary<(string, int), decimal[]>();
            }

            if (_candleBuffer == null)
            {
                _candleBuffer = new Dictionary<(string, int), BacktestingCandle[]>();
            }
        }

        /// <summary>
        /// Gets the pre-calculated buffer with the highest high of a certain number of past candles.
        /// </summary>
        /// <param name="pair">The trading pair to consider.</param>
        /// <param name="numberOfCandles">The number of candles to aggregate.</param>
        /// <returns>Complete highest high buffer.</returns>
        public decimal[] GetHighestHighs(TradingPair pair, int numberOfCandles)
        {
            if (!_highestHighBuffer.ContainsKey((pair.ToString(), numberOfCandles)))
            {
                _logger.LogCritical($"Building highest high buffer for {pair} with size {numberOfCandles}");
                var candles = GetCandles(pair);
                _highestHighBuffer[(pair.ToString(), numberOfCandles)] =
                    BuildHighestHighBuffer(candles, numberOfCandles);
            }

            return _highestHighBuffer[(pair.ToString(), numberOfCandles)];
        }

        /// <summary>
        /// Gets the pre-calculated buffer with the highest high of a certain number of past candles.
        /// </summary>
        /// <param name="pair">The TradingPair to consider.</param>
        /// <param name="channelWidth">The width of the candles.</param>
        /// <returns>Complete compressed candle buffer.</returns>
        public BacktestingCandle[] GetCandles(TradingPair pair, int channelWidth)
        {
            if (!_candleBuffer.ContainsKey((pair.ToString(), channelWidth)))
            {
                _logger.LogCritical($"Building compressed candle buffer for {pair} with size {channelWidth}");
                var candles = GetCandles(pair);
                _candleBuffer[(pair.ToString(), channelWidth)] =
                    BuildCandleBuffer(candles, channelWidth);
            }

            return _candleBuffer[(pair.ToString(), channelWidth)];
        }

        /// <summary>
        /// Gets the pre-calculated buffer with the lowest low of a certain number of candles.
        /// </summary>
        /// <param name="pair">the TradingPair to consider.</param>
        /// <param name="numberOfCandles">The number of candles to aggregate.</param>
        /// <returns>Complete lowest low buffer.</returns>
        public decimal[] GetLowestLow(TradingPair pair, int numberOfCandles)
        {
            if (!_lowestLowBuffer.ContainsKey((pair.ToString(), numberOfCandles)))
            {
                _logger.LogCritical($"Building lowest low buffer for {pair} with size {numberOfCandles}");
                var candles = GetCandles(pair);
                _lowestLowBuffer[(pair.ToString(), numberOfCandles)] =
                    BuildLowestLowBuffer(candles, numberOfCandles);
            }

            return _lowestLowBuffer[(pair.ToString(), numberOfCandles)];
        }

        private static decimal[] BuildBuffer(BacktestingCandle[] candles, int channelWidth, IComparer<decimal> comparer, Func<BacktestingCandle, decimal> selector)
        {
            var result = new decimal[candles.Length];
            var temp = new SortedDictionary<decimal, int>(comparer);

            // Logically equivalent to:
            // candles.Skip(i - channelWidth).Take(channelWidth).Max(x => x.High)
            // but in O(n * log(channelWidth)) as opposed to O(n * channelWidth)
            for (int i = 0; i < candles.Length; i++)
            {
                // Remove all outdated candles from the temp collection. ~O(log(channelWidth))
                while (temp.Any() && temp.First().Value <= i - channelWidth)
                {
                    temp.Remove(temp.First().Key);
                }

                var high = selector(candles[i]);
                if (temp.ContainsKey(high))
                {
                    // Extend time to life of the price entry. ~O(log(channelWidth))
                    temp[high] = i;
                }
                else
                {
                    // Add price entry.
                    temp.Add(high, i);
                }

                // Add the highest entry in the temp collection. O(1)
                result[i] = temp.First().Key;
            }

            return result;
        }

        private static decimal[] BuildHighestHighBuffer(BacktestingCandle[] candles, int channelWidth)
            => BuildBuffer(candles, channelWidth, new DescendingComparer(), x => x.High);

        private static decimal[] BuildLowestLowBuffer(BacktestingCandle[] candles, int channelWidth)
            => BuildBuffer(candles, channelWidth, new AscendingComparer(), x => x.Low);

        private static BacktestingCandle[] BuildCandleBuffer(BacktestingCandle[] candles, int channelWidth)
        {
            var localSize = Configuration.Instance.CandleWidth;
            if (channelWidth < localSize || channelWidth % localSize != 0)
            {
                throw new InvalidOperationException($"Cannot build a buffer with size {channelWidth} given the configured {localSize}");
            }

            int ratio = channelWidth / localSize;

            // Skip the last few candles that cannot be compressed.
            int excess = candles.Length % ratio;

            // Compress the candles with the ascending flag
            return DataProviderUtilities.CompressCandles(candles.SkipLast(excess).ToArray(), ratio, true);
        }

        /// <summary>
        /// Get all the candles from the buffer.
        /// </summary>
        /// <param name="pair">The trading pair to fetch the candles for.</param>
        /// <returns>An array of candles.</returns>
        private BacktestingCandle[] GetCandles(TradingPair pair)
        {
            if (!_buffers.ContainsKey(pair.ToString()))
            {
                _logger.LogCritical($"Building a new buffer for {pair}");
                _buffers.Add(
                    pair.ToString(),
                    _db.Candles.AsNoTracking()
                        .Where(x => x.TradingPair == pair.ToString())
                        .OrderBy(x => x.Timestamp)
                        .ToArray());
                _logger.LogCritical($"Done building the buffer for {pair}");
            }

            return _buffers[pair.ToString()];
        }

        /// <summary>toa
        /// An inverse comparer to maintain a descending ordered dictionary.
        /// </summary>
        private class DescendingComparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y) => y.CompareTo(x);
        }

        /// <summary>
        /// An inverse comparer to maintain a descending ordered dictionary.
        /// </summary>
        private class AscendingComparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y) => x.CompareTo(y);
        }
    }
}