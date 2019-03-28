using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting
{
    /// <summary>
    /// Mocking class for faking communication with a remote exchange.
    /// </summary>
    internal class BacktestCommunicationService : ExchangeCommunications
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestCommunicationService"/> class.
        /// </summary>
        /// <param name="factory">Injected loggin service.</param>
        public BacktestCommunicationService(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger(GetType());

            RemotePortfolio = Configuration.Instance.EnabledAlgorithm.Allocation;
        }

        /// <summary>
        /// Gets the remote portfolio of the backtest exchange.
        /// </summary>
        public Portfolio RemotePortfolio { get; }

        /// <inheritdoc />
        protected override void Startup() => Expression.Empty();
    }
}