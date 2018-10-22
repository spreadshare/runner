using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Provides trading capabilities.
    /// </summary>
    internal class TradingProvider : ITradingProvider
    {
        private readonly IExchangeTradingProvider _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of trading provider</param>
        public TradingProvider(IExchangeTradingProvider implementation)
        {
            _implementation = implementation;
        }

        /// <summary>
        /// Gets the portfolio associated with an algorithm
        /// </summary>
        /// <returns>Response object indicating success or not</returns>
        /// TODO: Make method algorithm specific
        public ResponseObject<Assets> GetPortfolio()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            decimal amount = 0;
            return _implementation.PlaceFullMarketOrder(pair, side, amount);
        }

        /// <inheritdoc />
        public ResponseObject CancelOrder(long orderId)
        {
            return _implementation.CancelOrder(orderId);
        }
    }
}
