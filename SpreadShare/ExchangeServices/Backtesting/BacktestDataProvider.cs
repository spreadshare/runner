using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Backtesting
{
    /// <summary>
    /// Provider implementation for backtest purposes.
    /// </summary>
    internal class BacktestDataProvider : AbstractDataProvider
    {
        private readonly BacktestTimerProvider _timerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="timerProvider">Used to keep track of time</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, BacktestTimerProvider timerProvider)
            : base(loggerFactory)
        {
            _timerProvider = timerProvider;
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