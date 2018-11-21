using System;
using System.Collections.Generic;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Provides data gathering capabilities.
    /// </summary>
    internal class DataProvider
    {
        private readonly AbstractDataProvider _implementation;
        private readonly AlgorithmSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of data provider</param>
        /// <param name="settings">The settings of the algorithm</param>
        public DataProvider(AbstractDataProvider implementation, AlgorithmSettings settings)
        {
            _implementation = implementation;
            _settings = settings;
        }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            return _implementation.GetCurrentPriceLastTrade(pair);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair)
        {
            return _implementation.GetCurrentPriceTopBid(pair);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair)
        {
            return _implementation.GetCurrentPriceTopAsk(pair);
        }

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">trading pair to obtain performance of</param>
        /// <param name="hoursBack">Number of hours to look back</param>
        /// <returns>A response object with the performance on success</returns>
        public ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            return _implementation.GetPerformancePastHours(pair, hoursBack);
        }

        /// <summary>
        /// Gets the top performing trading pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">Number of hours to look back</param>
        /// <returns>Top performing trading pair</returns>
        public ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            return _implementation.GetTopPerformance(pairs, hoursBack);
        }

        /// <summary>
        /// Gets a value estimation of a portfolio
        /// </summary>
        /// <param name="portfolio">portfolio</param>
        /// <returns>value in base currency</returns>
        public decimal ValuatePortfolioInBaseCurrency(Portfolio portfolio)
        {
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
                decimal price = _implementation.GetCurrentPriceLastTrade(pair).Data;
                sum += (balance.Free + balance.Locked) * price;
            }

            return sum;
        }
    }
}
