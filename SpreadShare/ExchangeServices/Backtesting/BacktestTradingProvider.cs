using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Backtesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        public BacktestTradingProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        /// <inheritdoc />
        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side, decimal amount)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            throw new System.NotImplementedException();
        }
    }
}