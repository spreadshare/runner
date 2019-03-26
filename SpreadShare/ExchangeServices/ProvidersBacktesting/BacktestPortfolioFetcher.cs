using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Fetches the backtest portfolio.
    /// </summary>
    internal class BacktestPortfolioFetcher : PortfolioFetcherService
    {
        private readonly BacktestCommunicationService _backtest;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestPortfolioFetcher"/> class.
        /// </summary>
        /// <param name="loggerFactory">To enable output.</param>
        /// <param name="backtest">For fetching the backtest portfolio.</param>
        public BacktestPortfolioFetcher(ILoggerFactory loggerFactory, BacktestCommunicationService backtest)
            : base(loggerFactory)
        {
            _backtest = backtest;
        }

        /// <inheritdoc />
        public override ResponseObject<Portfolio> GetPortfolio()
        {
            return new ResponseObject<Portfolio>(_backtest.RemotePortfolio);
        }
    }
}