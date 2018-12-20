using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
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
        /// Initializes a new instance of the <see cref="BinanceTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream</param>
        /// <param name="communications">For communication with Binance</param>
        public BinanceTradingProvider(ILoggerFactory loggerFactory, BinanceCommunicationsService communications)
            : base(loggerFactory)
        {
            _communications = communications;
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
                orderType: BinanceUtilities.ToInternal(order.Type),
                createdTimeStamp: DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                setPrice: 0, // This information is unknown for market orders
                side: side,
                pair: pair,
                setQuantity: quantity)
            {
                Status = OrderUpdate.OrderStatus.Filled,
                FilledQuantity = query.Data.ExecutedQuantity,
                FilledTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            // Prevent division by zero
            if (order.ExecutedQuantity == 0)
            {
                Logger.LogWarning($"Market order could not be priced --> {JsonConvert.SerializeObject(order)}");
                return new ResponseObject<OrderUpdate>(ResponseCode.Success, result);
            }

            // Calculate the price
            result.AverageFilledPrice = order.CummulativeQuoteQuantity / query.Data.ExecutedQuantity;

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

            #pragma warning disable SA1118
            return query.Success
                ? new ResponseObject<OrderUpdate>(
                    ResponseCode.Success,
                    new OrderUpdate(
                        query.Data.OrderId,
                        tradeId,
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

            var query = BinanceUtilities.RetryMethod(
                () => client.CancelOrder(pair.ToString(), orderId, null, null, _communications.ReceiveWindow),
                Logger);

            if (query.Success)
            {
                return new ResponseObject(ResponseCode.Error, query.Message);
            }

            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> GetOrderInfo(TradingPair pair, long orderId)
        {
            throw new NotImplementedException();
        }
    }
}
