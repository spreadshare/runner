using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Abstract specification of a trading provider.
    /// </summary>
    internal abstract class AbstractTradingProvider : Observable<OrderUpdate>
    {
        /// <summary>
        /// Create identifiable output.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// A list of orders pending events.
        /// </summary>
        protected Dictionary<long, OrderUpdate> _watchList;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream</param>
        protected AbstractTradingProvider(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            _watchList = new Dictionary<long, OrderUpdate>();
        }

        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">trading pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <param name="amount">The amount to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public abstract ResponseObject<decimal> PlaceFullMarketOrder(TradingPair pair, OrderSide side, decimal amount);

        /// <summary>
        /// Place a limit order at the given price.
        /// </summary>
        /// <param name="pair">trading pair</param>
        /// <param name="side">buy or sell order</param>
        /// <param name="amount">amount of non base currency</param>
        /// <param name="price">price to set the order at</param>
        /// <returns>A response object indicating the status of the limit order    </returns>
        public abstract ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal amount, decimal price);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="pair">The trading pair for which the order is set</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public abstract ResponseObject CancelOrder(TradingPair pair, long orderId);
    }
}