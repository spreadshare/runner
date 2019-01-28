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
            var rounded = pair.RoundToTradable(quantity);

            // Attempt to place the order on Binance
            var query = client.PlaceOrder(
                symbol: pair.ToString(),
                side: BinanceUtilities.ToExternal(side),
                type: OrderType.Market,
                quantity: rounded,
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
                Logger.LogError($"Placing market order {side} {rounded} {pair.Left} failed! --> {query.Error.Message}");
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
                setQuantity: quantity)
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

            var query = client.PlaceOrder(
                symbol: pair.ToString(),
                side: BinanceUtilities.ToExternal(side),
                type: OrderType.Limit,
                quantity: quantity,
                newClientOrderId: null,
                price: price,
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
                        price,
                        side,
                        pair,
                        quantity))
                : ResponseCommon.OrderPlacementFailed(query.Error.Message);
            #pragma warning disable SA1118
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            throw new NotImplementedException();
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
