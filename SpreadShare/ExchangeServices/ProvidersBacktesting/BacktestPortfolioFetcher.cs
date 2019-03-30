using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Fetches the backtest portfolio.
    /// </summary>
    internal class BacktestPortfolioFetcher : IPortfolioFetcherService
    {
        private readonly Portfolio _portfolio;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestPortfolioFetcher"/> class.
        /// </summary>
        public BacktestPortfolioFetcher()
        {
            _portfolio = Configuration.Instance.EnabledAlgorithm.Allocation;
        }

        /// <inheritdoc />
        public ResponseObject<Portfolio> GetPortfolio()
        {
            return new ResponseObject<Portfolio>(_portfolio);
        }
    }
}