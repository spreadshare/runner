using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Provides trading capabilities for Binance.
    /// </summary>
    internal class BinanceTradingProvider : AbstractTradingProvider
    {
        private readonly BinanceCommunicationsService _communications;

        /// <summary>
        /// This queue is used to cache orders until the next clock tick. It is also used to confirm order placements.
        /// </summary>
        private readonly ConcurrentQueue<OrderUpdate> _orderCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream.</param>
        /// <param name="communications">For communication with Binance.</param>
        /// <param name="timer">Timer for subscribing to periodic updates.</param>
        public BinanceTradingProvider(ILoggerFactory loggerFactory, BinanceCommunicationsService communications, TimerProvider timer)
            : base(loggerFactory, timer)
        {
            _communications = communications;
            _orderCache = new ConcurrentQueue<OrderUpdate>();

            // Push order updates from the websocket in a concurrent queue
            communications.Subscribe(new ConfigurableObserver<OrderUpdate>(
                order => _orderCache.Enqueue(order),
                () => { },
                e => { }));
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
        {
            var client = _communications.Client;
            var realQuantity = pair.RoundToTradable(quantity);

            // Attempt to place the order on Binance
            var query = client.PlaceOrder(
                symbol: pair.ToString(),
                side: BinanceUtilities.ToExternal(side),
                type: OrderType.Market,
                quantity: realQuantity,
                newClientOrderId: null,
                price: null,
                timeInForce: null,
                stopPrice: null,
                icebergQty: null,
                orderResponseType: null,
                (int)_communications.ReceiveWindow);

            // Report failure of placing market order
            if (!query.Success)
            {
                Logger.LogError($"Placing market order {side} {realQuantity} {pair.Left} failed! --> {query.Error.Message}");
                return new ResponseObject<OrderUpdate>(ResponseCode.Error, query.Error.Message);
            }

            var order = query.Data;

            // Create an order update with known information
            OrderUpdate result = new OrderUpdate(
                orderId: order.OrderId,
                tradeId: tradeId,
                orderStatus: OrderUpdate.OrderStatus.Filled,
                orderType: BinanceUtilities.ToInternal(order.Type),
                createdTimeStamp: DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                setPrice: 0, // This information is unknown for market orders
                side: side,
                pair: pair,
                setQuantity: realQuantity)
            {
                FilledQuantity = order.ExecutedQuantity,
                FilledTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                AverageFilledPrice = HelperMethods.SafeDiv(order.CummulativeQuoteQuantity, order.ExecutedQuantity),
            };

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, result);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            var client = _communications.Client;
            var realQuantity = pair.RoundToTradable(quantity);
            var realPrice = pair.RoundToPriceable(price);

            var query = client.PlaceOrder(
                symbol: pair.ToString(),
                side: BinanceUtilities.ToExternal(side),
                type: OrderType.Limit,
                quantity: realQuantity,
                newClientOrderId: null,
                price: realPrice,
                timeInForce: TimeInForce.GoodTillCancel,
                stopPrice: null,
                icebergQty: null,
                orderResponseType: null,
                receiveWindow: (int)_communications.ReceiveWindow);

            // Allow nested argument chopping
            #pragma warning disable SA1118
            return query.Success
                ? new ResponseObject<OrderUpdate>(
                    ResponseCode.Success,
                    new OrderUpdate(
                        query.Data.OrderId,
                        tradeId,
                        OrderUpdate.OrderStatus.New,
                        OrderUpdate.OrderTypes.Limit,
                        DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        realPrice,
                        side,
                        pair,
                        realQuantity))
                : ResponseCommon.OrderPlacementFailed(query.Error.Message);
            #pragma warning disable SA1118
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            var client = _communications.Client;
            decimal limitPrice;
            if (side == OrderSide.Sell)
            {
                // Set the limit price extremely low -> sell immediately for the best price.
                // 5% is an arbitrary number that is probably more than the spread, but is not
                // rejected by Binance for deviating too much from the current price.
                limitPrice = price * 0.95M;
            }
            else
            {
                // Skew the quantity and the price -> buy immediately for the best price.
                // Quantity must scale inverse because (quantity * price) is the amount that needs to
                // be locked. You cannot lock more assets than you have.
                // 2% is hardcoded on purpose because it is unlikely to change.
                limitPrice = price * 1.02M;
                quantity /= 1.02M;
            }

            var realQuantity = pair.RoundToTradable(quantity);
            var realLimitPrice = pair.RoundToPriceable(limitPrice);
            var realStopPrice = pair.RoundToPriceable(price);

            var query = client.PlaceOrder(
                symbol: pair.ToString(),
                side: BinanceUtilities.ToExternal(side),
                type: OrderType.StopLossLimit,
                quantity: realQuantity,
                newClientOrderId: null,
                price: realLimitPrice,
                timeInForce: TimeInForce.GoodTillCancel,
                stopPrice: realStopPrice,
                icebergQty: null,
                orderResponseType: null,
                receiveWindow: (int)_communications.ReceiveWindow);

            // Allow nested argument chopping
            #pragma warning disable SA1118
            return query.Success
                ? new ResponseObject<OrderUpdate>(
                    ResponseCode.Success,
                    new OrderUpdate(
                        query.Data.OrderId,
                        tradeId,
                        OrderUpdate.OrderStatus.New,
                        OrderUpdate.OrderTypes.StopLoss,
                        DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        realLimitPrice,
                        side,
                        pair,
                        realQuantity)
                    {
                        StopPrice = realStopPrice,
                    })
                : ResponseCommon.OrderPlacementFailed(query.Error.Message);
            #pragma warning disable SA1118
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            var client = _communications.Client;

            var query = client.CancelOrder(
                symbol: pair.ToString(),
                orderId: orderId,
                origClientOrderId: null,
                newClientOrderId: null,
                receiveWindow: _communications.ReceiveWindow);

            if (!query.Success)
            {
                return new ResponseObject(ResponseCode.Error, query.Error.Message);
            }

            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> WaitForOrderStatus(long orderId, OrderUpdate.OrderStatus status)
        {
            var values = _orderCache.ToList();
            foreach (var order in values)
            {
                if (order.OrderId == orderId && order.Status == status)
                {
                    return new ResponseObject<OrderUpdate>(order);
                }
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.NotFound);
        }

        /// <inheritdoc/>
        public override ResponseObject<OrderUpdate> GetOrderInfo(long orderId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public override void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public override void OnNext(long value)
        {
            // Flush the queue of pending order updates
            while (_orderCache.TryDequeue(out var order))
            {
                UpdateObservers(order);
            }
        }
    }
}
