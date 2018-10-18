using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Binance
{
    /// <summary>
    /// Provides trading capabilities for Binance.
    /// </summary>
    internal class BinanceTradingProvider : ITradingProvider
    {
        /// <inheritdoc />
        public ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject CancelOrder(long orderId)
        {
            throw new NotImplementedException();
        }
    }
}
