using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Concrete implementation of the IPortfolioFetcherService that fetches
    /// portfolio's from users.
    /// </summary>
    internal class PortfolioFetcherService : IPortfolioFetcherService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherService"/> class.
        /// </summary>
        public PortfolioFetcherService()
        {
            GetPortfolio();
        }

        /// <inheritdoc />
        public ResponseObject<Assets> GetPortfolio()
        {
            throw new System.NotImplementedException();
        }
    }
}
