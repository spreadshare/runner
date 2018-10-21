using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Abstract specification of a trading provider.
    /// </summary>
    internal abstract class AbstractTradingProvider : ITradingProvider
    {
        /// <summary>
        /// Create identifiable output.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream</param>
        protected AbstractTradingProvider(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public abstract ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side);

        /// <inheritdoc />
        public abstract ResponseObject CancelOrder(CurrencyPair pair, long orderId);
    }
}