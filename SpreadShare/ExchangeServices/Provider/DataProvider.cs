using System;
using System.Collections.Generic;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Provides data gathering capabilities.
    /// </summary>
    internal class DataProvider : IDataProvider
    {
        private readonly IDataProvider _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of data provider</param>
        public DataProvider(IDataProvider implementation)
        {
            _implementation = implementation;
        }

        /// <inheritdoc />
        public ResponseObject<Assets> GetPortfolio()
        {
            return _implementation.GetPortfolio();
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair)
        {
            return _implementation.GetCurrentPriceLastTrade(pair);
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair)
        {
            return _implementation.GetCurrentPriceTopBid(pair);
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair)
        {
            return _implementation.GetCurrentPriceTopAsk(pair);
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime)
        {
            return _implementation.GetPerformancePastHours(pair, hoursBack, endTime);
        }

        /// <inheritdoc />
        public ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime)
        {
            return _implementation.GetTopPerformance(pairs, hoursBack, endTime);
        }
    }
}
