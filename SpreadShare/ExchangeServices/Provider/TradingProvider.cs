using System;
using Binance.Net.Objects;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Provides trading capabilities.
    /// </summary>
    internal class TradingProvider : ITradingProvider
    {
        private readonly ITradingProvider _implementation;
        private readonly WeakAllocationManager _allocationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of trading provider</param>
        /// <param name="allocationManager">Provides portfolio access</param>
        public TradingProvider(ITradingProvider implementation, WeakAllocationManager allocationManager)
        {
            _implementation = implementation;
            _allocationManager = allocationManager;
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
            return _implementation.PlaceFullMarketOrder(pair, side);
        }

        /// <inheritdoc />
        public ResponseObject CancelOrder(long orderId)
        {
            return _implementation.CancelOrder(orderId);
        }
    }
}
