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
        /// <param name="exchangeSpecification">Specifies which exchange is used</param>
        ResponseObject<Assets> GetPortfolio(IExchangeSpecification exchangeSpecification);

        /// <summary>
        /// Gets the portfolio of the user.
        /// </summary>
        /// <returns>The portfolio</returns>
        /// <param name="exchange">Exchange to be used</param>
        ResponseObject<Assets> GetPortfolio(Exchange exchange);
    }
}
