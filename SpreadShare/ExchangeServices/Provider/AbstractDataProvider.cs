using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Abstract specification of a data provider.
    /// </summary>
    internal abstract class AbstractDataProvider : IDataProvider
    {
        /// <summary>
        /// Create identifiable output
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream </param>
        public AbstractDataProvider(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public abstract ResponseObject<Assets> GetPortfolio();

        /// <inheritdoc />
        public abstract ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair);

        /// <inheritdoc />
        public abstract ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair);

        /// <inheritdoc />
        public abstract ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair);

        /// <inheritdoc />
        public abstract ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime);

        /// <inheritdoc />
        public abstract ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime);
    }
}