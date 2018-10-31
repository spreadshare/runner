﻿using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Provides trading capabilities for Binance.
    /// </summary>
    internal class BinanceTradingProvider : AbstractTradingProvider, IExchangeSpecification
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
        public override ResponseObject<decimal> PlaceFullMarketOrder(CurrencyPair pair, Models.OrderSide side, decimal amount)
        {
            var client = _communications.Client;

            var query = client.PlaceOrder(pair.ToString(), BinanceUtilities.ToExternal(side), OrderType.Market, amount);
            if (query.Success)
            {
                return new ResponseObject<decimal>(ResponseCode.Success, query.Data.ExecutedQuantity);
            }

            Logger.LogWarning(query.ToString());

            Logger.LogWarning($"Placing market order {side} {amount}{pair} failed");
            return new ResponseObject<decimal>(ResponseCode.Error, 0.0M);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
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
        public Exchange GetExchangeType() => Exchange.Binance;
    }
}
