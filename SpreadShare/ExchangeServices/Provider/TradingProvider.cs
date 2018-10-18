using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Provides trading capabilities.
    /// </summary>
    internal class TradingProvider : ITradingProvider
    {
        private readonly ITradingProvider _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingProvider"/> class.
        /// </summary>
        /// <param name="implementation">Exchange implementation of trading provider</param>
        public TradingProvider(ITradingProvider implementation)
        {
            _implementation = implementation;
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
