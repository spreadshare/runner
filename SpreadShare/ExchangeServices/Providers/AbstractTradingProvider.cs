using System;
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
    internal abstract class AbstractTradingProvider : Observable<OrderUpdate>, IObserver<long>
    {
        /// <summary>
        /// Create identifiable output.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// A list of orders pending events.
        /// </summary>
        protected Dictionary<long, OrderUpdate> WatchList;

        /// <summary>
        /// Timer provider, used to subscribe to periodic updates
        /// </summary>
        protected TimerProvider Timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream</param>
        /// <param name="timer">Timer provider for </param>
        protected AbstractTradingProvider(ILoggerFactory loggerFactory, TimerProvider timer)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            WatchList = new Dictionary<long, OrderUpdate>();
            Timer = timer;
            timer.Subscribe(this);
        }

        /// <summary>
        /// Places market order with the full quantity of given pair
        /// </summary>
        /// <param name="pair">TradingPair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <param name="quantity">The quantity to buy or sell</param>
        /// <param name="tradeId">The id of the trade</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public abstract ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId);

        /// <summary>
        /// Place a limit order at the given price.
        /// </summary>
        /// <param name="pair">TradingPair</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="quantity">Quantity of non base currency</param>
        /// <param name="price">Price to set the order at</param>
        /// <param name="tradeId">The id of the trade</param>
        /// <returns>A response object indicating the status of the limit order</returns>
        public abstract ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId);

        /// <summary>
        /// Place a stoploss order at the given price
        /// </summary>
        /// <param name="pair">TradingPair</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="quantity">Quantity of non base currency</param>
        /// <param name="price">Price to set the order at</param>
        /// <param name="tradeId">The id of the trade</param>
        /// <returns>A response object indicating the status of the stoplos order</returns>
        public abstract ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="pair">The TradingPair for which the order is set</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public abstract ResponseObject CancelOrder(TradingPair pair, long orderId);

        /// <summary>
        /// Gets the info regarding an order
        /// </summary>
        /// <param name="pair">TradingPair</param>
        /// <param name="orderId">The id of the order</param>
        /// <returns>OrderUpdate containing the state of an order</returns>
        public abstract ResponseObject<OrderUpdate> GetOrderInfo(TradingPair pair, long orderId);

        /// <inheritdoc />
        public abstract void OnCompleted();

        /// <inheritdoc />
        public abstract void OnError(Exception error);

        /// <inheritdoc />
        public abstract void OnNext(long value);
    }
}