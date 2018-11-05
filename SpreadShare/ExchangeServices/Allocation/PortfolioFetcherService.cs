using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Concrete implementation of the IPortfolioFetcherService that fetches
    /// portfolio's from users.
    /// </summary>
    internal class PortfolioFetcherService : IPortfolioFetcherService
    {
        private readonly ILogger _logger;
        private readonly BinanceCommunicationsService _binance;
        private readonly BacktestCommunicationService _backtest;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        /// <param name="binance">Provides access to <binance cref="BinanceCommunicationsService"/></param>
        /// <param name="backtest">Provides access to the <backtest cref="BacktestCommunicationService"/></param>
        public PortfolioFetcherService(
            ILoggerFactory loggerFactory,
            BinanceCommunicationsService binance,
            BacktestCommunicationService backtest)
        {
            _logger = loggerFactory.CreateLogger<PortfolioFetcherService>();
            _binance = binance;
            _backtest = backtest;
        }

        /// <inheritdoc />
        public ResponseObject<Portfolio> GetPortfolio(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.Binance:
                    return GetBinancePortfolio();
                case Exchange.Backtesting:
                    return GetBacktestingPortfolio();
                default:
                    throw new InvalidEnumArgumentException(
                        exchange + "not found!");
            }
        }

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        private ResponseObject<Portfolio> GetBinancePortfolio()
        {
            var accountInfo = _binance.Client.GetAccountInfo();
            if (!accountInfo.Success)
            {
                _logger.LogCritical($"Could not get assets: {accountInfo.Error.Message}");
                return new ResponseObject<Portfolio>(ResponseCode.Error);
            }

            // Map to general Balance datatype for parsing to assets object.
            var values = accountInfo.Data.Balances.Where(x => x.Total > 0.0M).ToDictionary(
                x => new Currency(x.Asset),
                    x => new Balance(
                        new Currency(x.Asset),
                        x.Free,
                        x.Locked));

            return new ResponseObject<Portfolio>(ResponseCode.Success, new Portfolio(values));
        }

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        private ResponseObject<Portfolio> GetBacktestingPortfolio()
        {
            return new ResponseObject<Portfolio>(ResponseCode.Success, _backtest.RemotePortfolio);
        }
    }
}
