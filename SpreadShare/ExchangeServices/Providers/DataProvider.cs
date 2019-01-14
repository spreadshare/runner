using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
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
        /// <param name="implementation">Exchange implementation of data provider.</param>
        /// <param name="settings">The settings of the algorithm.</param>
        public DataProvider(AbstractDataProvider implementation, AlgorithmSettings settings)
        {
            _implementation = implementation;
            _settings = settings;
        }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            return _implementation.GetCurrentPriceLastTrade(pair);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair)
        {
            return _implementation.GetCurrentPriceTopBid(pair);
        }

        /// <summary>
        /// Get the current price of a trading pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'.
        /// </summary>
        /// <param name="pair">The trading pair.</param>
        /// <returns>The current price.</returns>
        public ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair)
        {
            return _implementation.GetCurrentPriceTopAsk(pair);
        }

        /// <summary>
        /// Gets past performance in the past hours.
        /// </summary>
        /// <param name="pair">Trading pair to obtain performance of.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>A response object with the performance on success.</returns>
        public ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            return _implementation.GetPerformancePastHours(pair, hoursBack);
        }

        /// <summary>
        /// Gets the top performing trading pair.
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate.</param>
        /// <param name="hoursBack">Number of hours to look back.</param>
        /// <returns>Top performing trading pair.</returns>
        public ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            return _implementation.GetTopPerformance(pairs, hoursBack);
        }

        /// <summary>
        /// Get the Average True Range of a certain number of minute candles.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="numberOfCandles">Number of candles to evaluate.</param>
        /// <returns>ResponseObject containing the Average True Range.</returns>
        public ResponseObject<decimal> GetAverageTrueRange(TradingPair pair, int numberOfCandles)
        {
            Guard.Argument(numberOfCandles).Require(x => x >= 2, x => $"Average true range needs at least 2 candles");
            var candles = _implementation.GetMinuteCandles(pair, numberOfCandles).Data.ToArray();
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

            return new ResponseObject<decimal>(ResponseCode.Success, (highLow + highPreviousClose + lowPreviousClose) / 3M);
        }

        /// <summary>
        /// Gets a value estimation of a portfolio.
        /// </summary>
        /// <param name="portfolio">Portfolio.</param>
        /// <returns>Value in base currency.</returns>
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
