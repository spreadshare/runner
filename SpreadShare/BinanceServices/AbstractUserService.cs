using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    /// <summary>
    /// Service responsible for fetching the portfolio and watching orders
    /// </summary>
    internal abstract class AbstractUserService : IUserService
    {
        /// <summary>
        /// Event handler for order updates
        /// </summary>
        /// TODO: Make the order update independent of Binance
        public EventHandler<BinanceStreamOrderUpdate> OrderUpdateHandler;

        /// <summary>
        /// Start the user service
        /// </summary>
        /// <returns>Whether the service was started successfully</returns>
        public abstract ResponseObject Start();

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        public abstract ResponseObject<Assets> GetPortfolio();

        /// <summary>
        /// Invokes event handler on order updates
        /// </summary>
        /// <param name="e">OrderUpdate</param>
        protected void OnOrderUpdate(BinanceStreamOrderUpdate e)
        {
             OrderUpdateHandler?.Invoke(this, e);
        }
    }
}