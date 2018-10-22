using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Alternative specification for implementation of trading providers.
    /// </summary>
    internal interface IExchangeTradingProvider
    {
        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">Currency pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <param name="amount">The amount to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side, decimal amount);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="pair">The currency pair for which the order is set</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        ResponseObject CancelOrder(CurrencyPair pair, long orderId);
    }
}