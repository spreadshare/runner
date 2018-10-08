using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    /// <summary>
    /// Service responsible for trading on an exchange
    /// </summary>
    internal abstract class AbstractTradingService : ITradingService
    {
        /// <summary>
        /// Indicated the status of a trade
        /// </summary>
        protected enum TradeState
        {
            /// <summary>
            /// Trade was received
            /// </summary>
            Received,

            /// <summary>
            /// Trade has been executed
            /// </summary>
            Executed,

            /// <summary>
            /// Trade has been cancelled
            /// </summary>
            Canceled,

            /// <summary>
            /// Trade has expired
            /// </summary>
            Expired,

            /// <summary>
            /// Trade has been rejected
            /// </summary>
            Rejected,

            /// <summary>
            /// Trade was not received
            /// </summary>
            Unreceived
        }

        /// <summary>
        /// Starts the trading service
        /// </summary>
        /// <returns>Whether the trading service was started succesfully</returns>
        public abstract ResponseObject Start();

        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">Currency pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public abstract ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public abstract ResponseObject CancelOrder(long orderId);

        /// <summary>
        /// Gets the current price of a currency pair
        /// </summary>
        /// <param name="pair">The currency pair</param>
        /// <returns>The current price</returns>
        public abstract ResponseObject<decimal> GetCurrentPrice(CurrencyPair pair);

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">Currency pair to obtain performance of</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>A response object with the performance on success</returns>
        public abstract ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime);

        /// <summary>
        /// Gets the top performing currency pair
        /// </summary>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>Top performing currency pair</returns>
        public abstract ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(double hoursBack, DateTime endTime);
    }
}