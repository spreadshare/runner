using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract specification of a data provider.
    /// </summary>
    internal abstract class AbstractDataProvider
    {
        /// <summary>
        /// Create identifiable output
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Create identifiable output
        /// </summary>
        protected readonly ExchangeCommunications ExchangeCommunications;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream </param>
        /// <param name="exchangeCommunications">Communicates with the exchange</param>
        protected AbstractDataProvider(ILoggerFactory loggerFactory, ExchangeCommunications exchangeCommunications)
        {
            ExchangeCommunications = exchangeCommunications;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets the current price of a trading pair by checking the last trade
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair);

        /// <summary>
        /// Get the current price of a trading pair by checking the top buy bid
        /// This value can be read as 'the most for which I can sell this'
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair);

        /// <summary>
        /// Get the current price of a trading pair by checking to sell bid
        /// This value can be read as 'the cheapest for which I can buy this'
        /// </summary>
        /// <param name="pair">The trading pair</param>
        /// <returns>The current price</returns>
        public abstract ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair);

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">trading pair to obtain performance of</param>
        /// <param name="hoursBack">SetAmount of hours to look back</param>
        /// <returns>A response object with the performance on success</returns>
        public abstract ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack);

        /// <summary>
        /// Gets the top performing trading pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">SetAmount of hours to look back</param>
        /// <returns>Top performing trading pair</returns>
        public abstract ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack);
    }
}