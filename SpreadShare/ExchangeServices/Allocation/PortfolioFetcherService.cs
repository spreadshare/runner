using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Concrete implementation of the IPortfolioFetcherService that fetches
    /// portfolio's from users.
    /// </summary>
    internal abstract class PortfolioFetcherService : IPortfolioFetcherService
    {
        /// <summary>
        /// Enables logging.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging.</param>
        protected PortfolioFetcherService(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<PortfolioFetcherService>();
        }

        /// <inheritdoc />
        public abstract ResponseObject<Portfolio> GetPortfolio();
    }
}
