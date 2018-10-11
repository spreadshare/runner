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
        /// Start the user service
        /// </summary>
        /// <returns>Whether the service was started successfully</returns>
        public abstract ResponseObject Start();

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        public abstract ResponseObject<Assets> GetPortfolio();
    }
}