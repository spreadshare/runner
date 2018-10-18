using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Interface for trading with an exchange
    /// </summary>
    internal interface ITradingProvider
    {
        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">Currency pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        ResponseObject CancelOrder(long orderId);
    }
}
