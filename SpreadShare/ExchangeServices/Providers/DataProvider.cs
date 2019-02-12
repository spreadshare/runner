using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Database;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Provides data gathering capabilities.
    /// </summary>
    internal class DataProvider
    {
        private readonly AlgorithmConfiguration _algorithmConfiguration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        /// <param name="factory">for generating output.</param>
        /// <param name="implementation">Exchange implementation of data provider.</param>
        /// <param name="settings">The settings of the algorithm.</param>
        public DataProvider(ILoggerFactory factory, AbstractDataProvider implementation, AlgorithmConfiguration settings)
        {
            Implementation = implementation;
            _algorithmConfiguration = settings;
            _logger = factory.CreateLogger(GetType());
        }

        private AbstractDataProvider Implementation { get; set; }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public decimal GetCurrentPriceLastTrade(TradingPair pair)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var query = HelperMethods.RetryMethod(() => Implementation.GetCurrentPriceLastTrade(pair), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public decimal GetCurrentPriceTopBid(TradingPair pair)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var query = HelperMethods.RetryMethod(() => Implementation.GetCurrentPriceTopBid(pair), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public decimal GetCurrentPriceTopAsk(TradingPair pair)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var query = HelperMethods.RetryMethod(() => Implementation.GetCurrentPriceTopAsk(pair), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Gets past performance in the past hours.
        /// </summary>
        /// <param name="pair">Trading pair to obtain performance of.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>A response object with the performance on success.</returns>
        public decimal GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(hoursBack).NotZero().NotNegative();
            var query = HelperMethods.RetryMethod(() => Implementation.GetPerformancePastHours(pair, hoursBack), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Gets the top performing trading pair.
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>Top performing trading pair.</returns>
        public Tuple<TradingPair, decimal> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            Guard.Argument(pairs).NotNull(nameof(pairs)).NotEmpty();
            Guard.Argument(hoursBack).NotNegative();
            var query = HelperMethods.RetryMethod(() => Implementation.GetTopPerformance(pairs, hoursBack), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Get a certain number of minute candles.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="numberOfCandles">Number of minute candles to request (>0).</param>
        /// <returns>Array of candles.</returns>
        public BacktestingCandle[] GetFiveMinuteCandles(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).NotZero().NotNegative();
            var query = HelperMethods.RetryMethod(() => Implementation.GetFiveMinuteCandles(pair, numberOfCandles), _logger);
            return query.Success
                ? query.Data.Length == numberOfCandles
                  ? query.Data
                  : throw new InvalidExchangeDataException($"Requested {numberOfCandles} but received {query.Data.Length}")
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Calculate the Average True Range (ATR) of certain pair given a number of candles, and a number
        /// of chunks they ought to be split in.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="candlesBack">The number of candles to use (must be a multiple of <see param="chunks"/>.</param>
        /// <param name="chunks">The number of chunks to divide the candles in before calculating the ATR.</param>
        /// <returns>The ATR value.</returns>
        public decimal GetAverageTrueRange(TradingPair pair, int candlesBack, int chunks = 5)
        {
            Guard.Argument(pair).NotNull(nameof(pair));

            Guard.Argument(chunks)
                .NotZero()
                .NotNegative();

            Guard.Argument(candlesBack)
                .NotZero()
                .NotNegative()
                .Require<ArgumentException>(
                    x => x % chunks == 0,
                    _ => $"{nameof(candlesBack)} has value {candlesBack} which is not a multiple of {nameof(chunks)} ({chunks})");

            // Retrieve one extra candle to gain the close.
            var rawCandles = GetFiveMinuteCandles(pair, candlesBack + 1);
            var originalCandles = rawCandles.SkipLast(1).ToArray();

            // Save close candles
            var closeCandle = rawCandles.Last();

            int compressionRatio = candlesBack / chunks;

            // Compress candles into {chunks} parts
            var candles = DataProviderUtilities.CompressCandles(originalCandles, compressionRatio);

            var trueRanges = new decimal[candles.Length];

            // Calculate maximum of edges over the series (closeCandle -> chunk[0] -> chunk[1] ... -> chunk[n])
            for (int i = candles.Length - 1; i >= 0; i--)
            {
                decimal highLow = Math.Abs(candles[i].High - candles[i].Low);

                // Edge case for first iteration
                decimal highPreviousClose = i == candles.Length - 1
                    ? Math.Abs(candles[i].High - closeCandle.Close)
                    : Math.Abs(candles[i].High - candles[i + 1].Close);

                // Edge case for the first iteration
                decimal lowPreviousClose = i == candles.Length - 1
                    ? Math.Abs(candles[i].Low - closeCandle.Close)
                    : Math.Abs(candles[i].Low - candles[i + 1].Close);

                trueRanges[i] = new[] { highLow, highPreviousClose, lowPreviousClose }.Max();
            }

            return trueRanges.Average();
        }

        /// <summary>
        /// Gets the Standard Moving Average (SMA) of a given pair, using a certain number of intervals, lasting a certain
        /// number of minutes.
        /// </summary>
        /// <param name="pair">The pair to calculate the SMA over.</param>
        /// <param name="candlesPerInterval">The number of minutes one interval should last.</param>
        /// <param name="numberOfIntervals">The number of intervals to consider.</param>
        /// <returns>The Standard Moving Average.</returns>
        public decimal GetStandardMovingAverage(TradingPair pair, int candlesPerInterval, int numberOfIntervals)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(candlesPerInterval)
                .NotNegative()
                .NotZero();

            Guard.Argument(numberOfIntervals)
                .NotNegative()
                .NotZero();

            // Calculate the total number of five minute candles required.
            int numberOfCandles = candlesPerInterval * numberOfIntervals;

            // Get all candles and compress them {candlesPerInterval} times resulting in {numberOfIntervals} compressed candles.
            var allCandles = GetFiveMinuteCandles(pair, numberOfCandles);
            var candles = DataProviderUtilities.CompressCandles(allCandles, candlesPerInterval);

            // Return the average of all closes.
            return candles.Average(x => x.Close);
        }

        /// <summary>
        /// Gets a value estimation of a portfolio.
        /// </summary>
        /// <param name="portfolio">Portfolio.</param>
        /// <returns>Value in base currency.</returns>
        public decimal ValuatePortfolioInBaseCurrency(Portfolio portfolio)
        {
            if (portfolio is null)
            {
                return 0M;
            }

            var balances = portfolio.AllBalances();
            decimal sum = 0;
            foreach (var balance in balances)
            {
                if (balance.Symbol == _algorithmConfiguration.BaseCurrency)
                {
                    sum += balance.Free + balance.Locked;
                    continue;
                }

                TradingPair pair = TradingPair.Parse(balance.Symbol, _algorithmConfiguration.BaseCurrency);
                decimal price = GetCurrentPriceLastTrade(pair);
                sum += (balance.Free + balance.Locked) * price;
            }

            return sum;
        }
    }
}
