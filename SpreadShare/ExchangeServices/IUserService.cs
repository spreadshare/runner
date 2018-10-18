using SpreadShare.Models;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// Interface for obtaining user information from an exchange
    /// </summary>
    internal interface IUserService
    {
        /// <summary>
        /// Start user service
        /// </summary>
        /// <returns>Whether the starting of the service was successful</returns>
        ResponseObject Start();

        /// <summary>
        /// Gets the portfolio of the authenticated user
        /// </summary>
        /// <returns>A response with assets if successful</returns>
        ResponseObject<Assets> GetPortfolio();
    }
}
