using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Fetches the backtest portfolio.
    /// </summary>
    internal class BacktestPortfolioFetcher : IPortfolioFetcherService
    {
        private readonly BacktestCommunicationService _backtest;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestPortfolioFetcher"/> class.
        /// </summary>
        /// <param name="backtest">For fetching the backtest portfolio.</param>
        public BacktestPortfolioFetcher(BacktestCommunicationService backtest)
        {
            _backtest = backtest;
        }

        /// <inheritdoc />
        public ResponseObject<Portfolio> GetPortfolio()
        {
            return new ResponseObject<Portfolio>(_backtest.RemotePortfolio);
        }
    }
}