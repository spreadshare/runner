using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract specification of a data provider.
    /// </summary>
    internal abstract class AbstractDataProvider
    {
        /// <summary>
        /// Create identifiable output.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream. </param>
        /// <param name="timerProvider">To keep track of the pivot for candle compressing.</param>
        protected AbstractDataProvider(ILoggerFactory loggerFactory, TimerProvider timerProvider)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            TimerProvider = timerProvider;
        }

        /// <summary>
        /// Gets or sets the TimerProvider.
        /// </summary>
        protected TimerProvider TimerProvider { get; set; }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair);

        /// <summary>
        /// Get the current price of a trading pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair);

        /// <summary>
        /// Get the current price of a trading pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair);

        /// <summary>
        /// Gets past performance in the past hours.
        /// </summary>
        /// <param name="pair">Trading pair to obtain performance of.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>A response object with the performance on success.</returns>
        public abstract ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack);

        /// <summary>
        /// Get candles of a custom size.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="numberOfCandles">Number of custom candles.</param>
        /// <param name="width">The width of the custom candle.</param>
        /// <returns>Array of custom candles.</returns>
        public virtual ResponseObject<BacktestingCandle[]> GetCustomCandles(TradingPair pair, int numberOfCandles, CandleWidth width)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var localCandleSize = (int)Configuration.Instance.CandleWidth;
            var targetCandleSize = (int)width;

            Guard.Argument(targetCandleSize)
                .Require<ArgumentOutOfRangeException>(
                    x => x >= localCandleSize,
                    x => $"Target candle size {x}min requires decompression given the configured candle size {localCandleSize}min")
                .Require<ArgumentOutOfRangeException>(
                    x => x % localCandleSize == 0,
                    x => $"Cannot compress candles from {x}min to {localCandleSize}min because {x} is not divible by {localCandleSize}");

            // Number of candles needed for the query
            var targetCandleCount = (targetCandleSize / localCandleSize) * numberOfCandles;
            var timespan = TimerProvider.CurrentTime - TimerProvider.Pivot;

            // Number of candles that are left after dividing by the target size (uncompleted batch)
            var padding = ((int)timespan.TotalMinutes % targetCandleSize) / localCandleSize;

            // Request the correct number of candles but skip the padding
            var candlesQuery = GetCandles(pair, targetCandleCount + padding);
            if (!candlesQuery.Success)
            {
                return candlesQuery;
            }

            var candles = candlesQuery.Data.Skip(padding).ToArray();
            return new ResponseObject<BacktestingCandle[]>(
                DataProviderUtilities.CompressCandles(candles, targetCandleSize / localCandleSize));
        }

        /// <summary>
        /// Get the highest high of a certain number of recent candles.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="width">The width of a candle (e.g. FiveMinutes).</param>
        /// <param name="numberOfCandles">The number of candles to utilize.</param>
        /// <returns>The highest high.</returns>
        public virtual ResponseObject<decimal> GetHighestHigh(TradingPair pair, CandleWidth width, int numberOfCandles)
        {
            var candles = GetCustomCandles(pair, numberOfCandles, width);
            return candles.Success
                ? new ResponseObject<decimal>(candles.Data.Max(x => x.High))
                : new ResponseObject<decimal>(ResponseCode.Error, candles.Message);
        }

        /// <summary>
        /// Get the lowest low of certain number of recent candles.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="width">The width of a candle (e.g. FiveMinutes).</param>
        /// <param name="numberOfCandles">The number of candles to utilize.</param>
        /// <returns>The lowest low.</returns>
        public virtual ResponseObject<decimal> GetLowestLow(TradingPair pair, CandleWidth width, int numberOfCandles)
        {
            var candles = GetCustomCandles(pair, numberOfCandles, width);
            return candles.Success
                ? new ResponseObject<decimal>(candles.Data.Min(x => x.Low))
                : new ResponseObject<decimal>(ResponseCode.Error, candles.Message);
        }

        /// <summary>
        /// Gets the top performing trading pair.
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>Top performing trading pair.</returns>
        public abstract ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack);

        /// <summary>
        /// Gets a certain number of candles with the configured size.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="limit">Number of candles to fetch.</param>
        /// <returns>ResponseObject containing a candle array.</returns>
        protected abstract ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit);
    }
}