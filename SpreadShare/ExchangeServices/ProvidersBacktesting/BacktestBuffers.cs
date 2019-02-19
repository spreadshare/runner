using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Caches database backtest data in memory for rapid backtesting.
    /// </summary>
    internal class BacktestBuffers
    {
        private static Dictionary<string, BacktestingCandle[]> _buffers;
        private static Dictionary<(string, int), decimal[]> _highestHighbuffer;

        private DatabaseContext _db;
        private ILogger _logger;

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

            if (_highestHighbuffer == null)
            {
                _highestHighbuffer = new Dictionary<(string, int), decimal[]>();
            }
        }

        /// <summary>
        /// Get all the candles from the buffer.
        /// </summary>
        /// <param name="pair">The trading pair to fetch the candles for.</param>
        /// <returns>An array of candles.</returns>
        public BacktestingCandle[] GetCandles(TradingPair pair)
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

        /// <summary>
        /// Gets the pre-calculated buffer with the highest high of a certain number of past candles.
        /// </summary>
        /// <param name="pair">The trading pair to consider.</param>
        /// <param name="numberOfCandles">The number of candles to aggregate.</param>
        /// <returns>Complete highest high buffer.</returns>
        public decimal[] GetHighestHighs(TradingPair pair, int numberOfCandles)
        {
            if (!_highestHighbuffer.ContainsKey((pair.ToString(), numberOfCandles)))
            {
                _logger.LogCritical($"Building highest high buffer for {pair} with size {numberOfCandles}");
                var candles = GetCandles(pair);
                _highestHighbuffer[(pair.ToString(), numberOfCandles)] =
                    BuildHighestHighBuffer(candles, numberOfCandles);
            }

            return _highestHighbuffer[(pair.ToString(), numberOfCandles)];
        }

        private static decimal[] BuildHighestHighBuffer(BacktestingCandle[] candles, int channelWidth)
        {
            var result = new decimal[candles.Length];
            var temp = new SortedDictionary<decimal, int>(new Comparer());
            var file = new StreamWriter("blegh.txt");

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

                var high = candles[i].High;
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

            file.Close();
            return result;
        }

        /// <summary>
        /// An inverse comparer to maintain a descending ordered dictionary.
        /// </summary>
        private class Comparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y) => y.CompareTo(x);
        }
    }
}