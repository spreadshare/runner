using System;
using System.Collections.Generic;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Provides data gathering capabilities.
    /// </summary>
    internal class DataProvider : IObservable<OrderUpdate>
    {
        private readonly AbstractDataProvider _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of data provider</param>
        public DataProvider(AbstractDataProvider implementation)
        {
            _implementation = implementation;
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<OrderUpdate> observer) => _implementation.Subscribe(observer);

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
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>A response object with the performance on success</returns>
        public ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack, DateTime endTime)
        {
            return _implementation.GetPerformancePastHours(pair, hoursBack, endTime);
        }

        /// <summary>
        /// Gets the top performing trading pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>Top performing trading pair</returns>
        public ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack, DateTime endTime)
        {
            return _implementation.GetTopPerformance(pairs, hoursBack, endTime);
        }
    }
}
