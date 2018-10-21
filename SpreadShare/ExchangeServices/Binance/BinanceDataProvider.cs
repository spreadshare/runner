using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Binance
{
    /// <summary>
    /// Provides data gathering capabilities for Binance.
    /// </summary>
    internal class BinanceDataProvider : AbstractDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream</param>
        public BinanceDataProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        /// <inheritdoc />
        public override ResponseObject<Assets> GetPortfolio()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime)
        {
            throw new NotImplementedException();
        }
    }
}
