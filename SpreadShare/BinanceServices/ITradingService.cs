using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    /// <summary>
    /// Interface for trading with an exchange
    /// </summary>
    internal interface ITradingService
    {
        /// <summary>
        /// Start trading service
        /// </summary>
        /// <returns>Whether the starting of the service was successful</returns>
        ResponseObject Start();
    }
}
