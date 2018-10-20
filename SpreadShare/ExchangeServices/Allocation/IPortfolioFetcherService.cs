using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Interface for services that fetch portfolio's of exchanges.
    /// </summary>
    internal interface IPortfolioFetcherService
    {
        /// <summary>
        /// Gets the portfolio of the user.
        /// </summary>
        /// <returns>The portfolio</returns>
        ResponseObject<Assets> GetPortfolio();
    }
}
