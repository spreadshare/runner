using System;
using System.Collections.Generic;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Interface for classes providing data gathering capabilities.
    /// </summary>
    internal interface IDataProvider
    {
        /// <summary>
        /// Gets the portfolio
        /// </summary>
        /// <returns>A portfolio instance</returns>
        ResponseObject<Assets> GetPortfolio();

        /// <summary>
        /// Gets the current price of a currency pair by checking the last trade
        /// </summary>
        /// <param name="pair">The currency pair</param>
        /// <returns>The current price</returns>
        ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair);

        /// <summary>
        /// Get the current price of a currency pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'
        /// </summary>
        /// <param name="pair">The currency pair</param>
        /// <returns>The current price</returns>
        ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair);

        /// <summary>
        /// Get the current price of a currency pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'
        /// </summary>
        /// <param name="pair">The currency pair</param>
        /// <returns>The current price</returns>
        ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair);

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">Currency pair to obtain performance of</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>A response object with the performance on success</returns>
        ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime);

        /// <summary>
        /// Gets the top performing currency pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>Top performing currency pair</returns>
        ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime);
    }
}
