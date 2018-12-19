using System;
using System.Linq;
using System.Threading;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
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
                pair.ToString(),
                BinanceUtilities.ToExternal(side),
                OrderType.Market,
                rounded,
                null,
                null,
                null,
                null,
                null,
                null,
                (int)_communications.ReceiveWindow);

            // Report failure of placing market order
            if (!query.Success)
            {
                Logger.LogError($"Placing market order {side} {rounded} {pair.Left} failed! --> {query.Error.Message}");
                return new ResponseObject<OrderUpdate>(ResponseCode.Error, query.Error.Message);
            }

            // Create an order update with known information
            OrderUpdate result = new OrderUpdate(
                query.Data.OrderId,
                tradeId,
                OrderUpdate.OrderTypes.Market,
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                0,
                side,
                pair,
                quantity)
            {
                Status = OrderUpdate.OrderStatus.Filled,
                FilledQuantity = query.Data.ExecutedQuantity,
                FilledTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };

            // Give Binance a short time to process the order
            Thread.Sleep(10);

            // Get the open orders from Binance, this should not contain the market order
            var orders = BinanceUtilities.RetryMethod(() => client.GetOpenOrders(result.Pair.ToString()), Logger);
            if (!orders.Success)
            {
                Logger.LogWarning($"Market order with id {result.OrderId} could not be confirmed");
            }
            else if (orders.Data.All(o => o.OrderId != result.OrderId))
            {
                // None of the open orders was the market order, it must have been filled
                result.Status = OrderUpdate.OrderStatus.Filled;
            }
            else
            {
                Logger.LogWarning($"Market order {result.OrderId} is not being reported as filled");
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, result);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            var client = _communications.Client;

            var query = client.PlaceOrder(
                pair.ToString(),
                BinanceUtilities.ToExternal(side),
                OrderType.Limit,
                quantity,
                null,
                price,
                null,
                null,
                null,
                null,
                (int) _communications.ReceiveWindow);

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
                : ResponseCommon.OrderPlacementFailed;

        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            // set alias for more readable code
            var client = _communications.Client;

            var query = client.CancelOrder(pair.ToString(), orderId);
            if (query.Success)
            {
                return new ResponseObject(ResponseCode.Error, query.Error.Message);
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
