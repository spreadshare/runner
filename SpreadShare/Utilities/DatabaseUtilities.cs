using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;
using Dawn;
using Microsoft.EntityFrameworkCore;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Utilities service for database meta-queries.
    /// </summary>
    internal class DatabaseUtilities
    {
        private readonly DatabaseContext _databaseContext;
        private readonly Dictionary<TradingPair, (long, long)> _timestampEdgesCache;
        private readonly List<TradingPair> _candlewidthCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseUtilities"/> class.
        /// </summary>
        /// <param name="databaseContext">The database context.</param>
        public DatabaseUtilities(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            _timestampEdgesCache = new Dictionary<TradingPair, (long, long)>();
            _candlewidthCache = new List<TradingPair>();
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
            Guard.Argument(pairs).NotNull();

            var minBeginVal = long.MinValue;
            var minEndVal = long.MaxValue;

            foreach (var pair in pairs)
            {
                long first, last;

                if (!_timestampEdgesCache.ContainsKey(pair))
                {
                    if (!_databaseContext.Candles.AsNoTracking().Any(x => x.TradingPair == pair.ToString()))
                    {
                        throw new Exception($"Database does not contain candles for {pair}");
                    }

                    // First case: lookup edges
                    first = _databaseContext.Candles.AsNoTracking()
                        .Where(x => x.TradingPair == pair.ToString())
                        .OrderBy(x => x.OpenTimestamp)
                        .First().OpenTimestamp;
                    last = _databaseContext.Candles.AsNoTracking()
                        .Where(x => x.TradingPair == pair.ToString())
                        .OrderBy(x => x.OpenTimestamp)
                        .Last().OpenTimestamp;

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
        /// <param name="pairs">The TradingPairs to take into account.</param>
        /// <param name="check">The expected width of a candle.</param>
        /// <exception cref="SpreadShare.Models.Exceptions.InvalidConfigurationException">Thrown when candles are not
        /// compatible with the CandleWidth property in the configuration.</exception>
        public void ValidateCandleWidth(List<TradingPair> pairs, int check)
        {
            Guard.Argument(pairs).NotNull().NotEmpty();

            var width = (int)TimeSpan.FromMinutes(check).TotalMilliseconds;

            foreach (var pair in pairs)
            {
                // Pair has already been checked
                if (_candlewidthCache.Contains(pair))
                {
                    continue;
                }

                // Take a small sample of candles.
                var sample = _databaseContext.Candles.AsNoTracking()
                    .Where(x => x.TradingPair == pair.ToString())
                    .OrderBy(x => x.OpenTimestamp)
                    .Take(10);

                // Check if the the difference in timestamps matches the given check.
                var subResult = sample.AsEnumerable()
                    .Pairwise((a, b) => b.OpenTimestamp - a.OpenTimestamp == width)
                    .All(x => x);

                if (!subResult)
                {
                    throw new InvalidConfigurationException(
                        $"Database candle interval for {pair} is not compatible with {Configuration.Instance.CandleWidth}");
                }

                // Cache previous positives
                _candlewidthCache.Add(pair);
            }
        }
    }
}