using Microsoft.Extensions.Logging;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting
{
    /// <summary>
    /// Mocking class for faking communication with a remote exchange
    /// </summary>
    internal class BacktestCommunicationService : ExchangeCommunications
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestCommunicationService"/> class.
        /// </summary>
        /// <param name="settings">Injected settings service</param>
        /// <param name="factory">Injected loggin service</param>
        public BacktestCommunicationService(ISettingsService settings, ILoggerFactory factory)
        {
            _logger = factory.CreateLogger(GetType());
            RemotePortfolio = (settings as SettingsService).BackTestSettings.InitialPortfolio;
        }

        /// <summary>
        /// Gets the remote portfolio of the backtest exchange.
        /// </summary>
        public Portfolio RemotePortfolio { get; }
    }
}