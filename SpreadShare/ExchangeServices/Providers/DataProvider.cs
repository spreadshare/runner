using System;
using System.Collections.Generic;
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

        // Setter is used with reflection in the tests.
        private AbstractDataProvider Implementation { get; set; }

        private int CandleWidth => _algorithmConfiguration.CandleWidth;

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
        /// Get a certain number of minute candles ordered from present -> past.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="numberOfCandles">Number of minute candles to request (>0).</param>
        /// <returns>Array of candles.</returns>
        public BacktestingCandle[] GetCandles(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).NotZero().NotNegative();

            var query = HelperMethods.RetryMethod(
                () => Implementation.GetCustomCandles(pair, numberOfCandles, CandleWidth), _logger, 5, 1000);
            return query.Success
                ? query.Data.Length == numberOfCandles
                  ? query.Data
                  : throw new InvalidExchangeDataException($"Requested {numberOfCandles} candles but received {query.Data.Length}")
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Gets the highest high of a certain number of candles.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="numberOfCandles">number of candles to consider.</param>
        /// <returns>The maximum value of the highs of all the candles.</returns>
        public decimal GetHighestHigh(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).NotZero().NotNegative();
            var query = HelperMethods.RetryMethod(
                () => Implementation.GetHighestHigh(pair, CandleWidth, numberOfCandles), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Gets the lowest low of a certain number of candles.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="numberOfCandles">number of candles to consider.</param>
        /// <returns>The minimum value of the lows of all the candles.</returns>
        public decimal GetLowestLow(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).NotZero().NotNegative();
            var query = HelperMethods.RetryMethod(
                () => Implementation.GetLowestLow(pair, CandleWidth, numberOfCandles), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
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
