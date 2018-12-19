using System;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
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
        public override ResponseObject<OrderUpdate> PlaceMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
        {
            var client = _communications.Client;
            var rounded = pair.RoundToTradable(quantity);
 
            // Attempt to place the order on Binance
            var query = client.PlaceOrder(pair.ToString(),
                BinanceUtilities.ToExternal(side),
                OrderType.Market,
                rounded,
                null,
                null,
                null,
                null,
                null,
                null,
                (int) _communications.ReceiveWindow);
 
            if (!query.Success)
            {
                Logger.LogError($"Placing market order {side} {rounded}{pair} failed! --> {query.Error.Message}");
                return new ResponseObject<OrderUpdate>(ResponseCode.Error, query.Error.Message);
            }

            OrderUpdate result = new OrderUpdate(query.Data.OrderId,
                tradeId,
                OrderUpdate.OrderTypes.Market,
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                query.Data.Price,
                side,
                pair,
                quantity)
            {
                FilledQuantity = query.Data.ExecutedQuantity
            };
 
            return new ResponseObject<OrderUpdate>(ResponseCode.Success, result);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            throw new System.NotImplementedException();
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
