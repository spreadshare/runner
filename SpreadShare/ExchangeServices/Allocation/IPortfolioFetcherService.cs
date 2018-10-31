using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

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
        /// <param name="exchange">Exchange to be used</param>
        ResponseObject<Portfolio> GetPortfolio(Exchange exchange);
    }
}