using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;
using Dawn;
using Microsoft.EntityFrameworkCore;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Utilities service for database meta-queries.
    /// </summary>
    internal class DatabaseUtilities
    {
        private readonly DatabaseContext _databaseContext;
        private readonly Dictionary<TradingPair, (long, long)> _timestampEdgesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseUtilities"/> class.
        /// </summary>
        /// <param name="databaseContext">The database context.</param>
        public DatabaseUtilities(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            _timestampEdgesCache = new Dictionary<TradingPair, (long, long)>();
        }

        /// <summary>
        /// Gets the instance of <see cref="DatabaseUtilities"/>.
        /// </summary>
        public static DatabaseUtilities Instance { get; private set; }

        /// <summary>
        /// Lift the current instance to the global singleton.
        /// </summary>
        public void Bind()
        {
            Instance = this;
        }

        /// <summary>
        /// Get the widest, valid begin- and end timestamp of a backtest given a set of TradingPairs.
        /// </summary>
        /// <param name="pairs">The TradingPairs to take into account.</param>
        /// <returns>The timestamp edges.</returns>
        public (long, long) GetTimeStampEdges(List<TradingPair> pairs)
        {
            Guard.Argument(_databaseContext.Candles).NotEmpty(x => $"Database contains no candles!");
            Guard.Argument(pairs).NotNull();

            long minBeginVal = 0;
            long minEndVal = long.MaxValue;
            foreach (var pair in pairs)
            {
                if (!_databaseContext.Candles.Any(x => x.TradingPair == pair.ToString()))
                {
                    throw new Exception($"Database does not contain candles for {pair}");
                }

                long first, last;

                // First case: lookup edges
                if (!_timestampEdgesCache.ContainsKey(pair))
                {
                    first = _databaseContext.Candles.OrderBy(x => x.Timestamp)
                        .First(x => x.TradingPair == pair.ToString()).Timestamp;
                    last = _databaseContext.Candles.OrderBy(x => x.Timestamp)
                        .Last(x => x.TradingPair == pair.ToString()).Timestamp;

                    // Save result in cache.
                    _timestampEdgesCache[pair] = (first, last);
                }
                else
                {
                    // Use cached result
                    (first, last) = _timestampEdgesCache[pair];
                }

                if (first > minBeginVal)
                {
                    minBeginVal = first;
                }

                if (last < minEndVal)
                {
                    minEndVal = last;
                }
            }

            return (minBeginVal, minEndVal);
        }

        /// <summary>
        /// Check if the database candles for a certain pair match an interval.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="check">The expected width of a candle.</param>
        /// <returns>Whether the database candles intervals matched the given CandleWidth.</returns>
        public bool ValidateCandleWidth(TradingPair pair, CandleWidth check)
        {
            Guard.Argument(pair).NotNull();

            int width = TimeSpan.FromMinutes((int)check).Milliseconds;

            // Take a small sample of candles.
            var sample = _databaseContext.Candles.AsNoTracking()
                .Where(x => x.TradingPair == pair.ToString())
                .OrderBy(x => x.Timestamp).Take(10);

            // Check if the the difference in timestamps matches the given check.
            return sample.AsEnumerable()
                .Pairwise((a, b) => b.Timestamp - a.Timestamp == width)
                .All(x => x);
        }
    }
}