using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging</param>
        /// <param name="binance">Provides access to <binance cref="BinanceCommunicationsService"/></param>
        public PortfolioFetcherService(ILoggerFactory loggerFactory, BinanceCommunicationsService binance)
        {
            _logger = loggerFactory.CreateLogger<PortfolioFetcherService>();
            _binance = binance;
        }

        /// <inheritdoc />
        public ResponseObject<Assets> GetPortfolio(IExchangeSpecification exchangeSpecification)
        {
            return GetPortfolio(exchangeSpecification.GetExchangeType());
        }

        /// <inheritdoc />
        public ResponseObject<Assets> GetPortfolio(Exchange exchange)
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
        private ResponseObject<Assets> GetBinancePortfolio()
        {
            var accountInfo = _binance.Client.GetAccountInfo();
            if (!accountInfo.Success)
            {
                _logger.LogCritical($"Could not get assets: {accountInfo.Error.Message}");
                return new ResponseObject<Assets>(ResponseCode.Error);
            }

            // Map to general ExchangeBalance datatype for parsing to assets object.
            var values = accountInfo.Data.Balances.Select(x => new ExchangeBalance(x.Asset, x.Free, x.Locked)).ToList();

            return new ResponseObject<Assets>(ResponseCode.Success, new Assets(values));
        }

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        private ResponseObject<Assets> GetBacktestingPortfolio()
        {
            throw new NotImplementedException();
        }
    }
}
