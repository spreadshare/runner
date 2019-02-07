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
        private readonly AbstractDataProvider _implementation;
        private readonly AlgorithmConfiguration _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        /// <param name="factory">for generating output.</param>
        /// <param name="implementation">Exchange implementation of data provider.</param>
        /// <param name="settings">The settings of the algorithm.</param>
        public DataProvider(ILoggerFactory factory, AbstractDataProvider implementation, AlgorithmConfiguration settings)
        {
            _implementation = implementation;
            _settings = settings;
            _logger = factory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public decimal GetCurrentPriceLastTrade(TradingPair pair)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var query = HelperMethods.RetryMethod(() => _implementation.GetCurrentPriceLastTrade(pair), _logger);
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
            var query = HelperMethods.RetryMethod(() => _implementation.GetCurrentPriceTopBid(pair), _logger);
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
            var query = HelperMethods.RetryMethod(() => _implementation.GetCurrentPriceTopAsk(pair), _logger);
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
            var query = HelperMethods.RetryMethod(() => _implementation.GetPerformancePastHours(pair, hoursBack), _logger);
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
            var query = HelperMethods.RetryMethod(() => _implementation.GetTopPerformance(pairs, hoursBack), _logger);
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
        public BacktestingCandle[] GetMinuteCandles(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).NotZero().NotNegative();
            var query = HelperMethods.RetryMethod(() => _implementation.GetMinuteCandles(pair, numberOfCandles), _logger);
            return query.Success
                ? query.Data
                : throw new ExchangeConnectionException(query.Message);
        }

        /// <summary>
        /// Get the Average True Range of a certain number of minute candles.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="numberOfCandles">Number of candles to evaluate.</param>
        /// <returns>ResponseObject containing the Average True Range.</returns>
        public decimal GetAverageTrueRange(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(numberOfCandles).Require(x => x >= 2, x => $"Average true range needs at least 2 candles");
            var candles = GetMinuteCandles(pair, numberOfCandles);

            decimal highLow = candles.Max(x => x.High - x.Low);
            decimal highPreviousClose = 0;
            decimal lowPreviousClose = 0;
            for (int i = 1; i < candles.Length; i++)
            {
                decimal highPreviousCloseValue = candles[i].High - candles[i - 1].Close;
                if (highPreviousCloseValue > highPreviousClose)
                {
                    highPreviousClose = highPreviousCloseValue;
                }

                decimal lowPreviousCloseValue = candles[i].Low - candles[i - 1].Close;
                if (lowPreviousCloseValue > lowPreviousClose)
                {
                    lowPreviousClose = lowPreviousCloseValue;
                }
            }

            return (highLow + highPreviousClose + lowPreviousClose) / 3M;
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
                if (balance.Symbol == _settings.BaseCurrency)
                {
                    sum += balance.Free + balance.Locked;
                    continue;
                }

                TradingPair pair = TradingPair.Parse(balance.Symbol, _settings.BaseCurrency);
                decimal price = GetCurrentPriceLastTrade(pair);
                sum += (balance.Free + balance.Locked) * price;
            }

            return sum;
        }
    }
}
