using System;
using System.Collections.Generic;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Binance
{
    /// <summary>
    /// Provides data gathering capabilities for Binance.
    /// </summary>
    internal class BinanceDataProvider : IDataProvider
    {
        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime)
        {
            throw new NotImplementedException();
        }
    }
}
