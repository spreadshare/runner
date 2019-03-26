using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Is able to fetch portfolio from binance.
    /// </summary>
    internal class BinancePortfolioFetcher : IPortfolioFetcherService
    {
        private readonly ILogger _logger;
        private readonly BinanceCommunicationsService _binance;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinancePortfolioFetcher"/> class.
        /// </summary>
        /// <param name="loggerFactory">To enable logging.</param>
        /// <param name="comms">Provides access to binance.</param>
        public BinancePortfolioFetcher(ILoggerFactory loggerFactory, BinanceCommunicationsService comms)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _binance = comms;
        }

        /// <inheritdoc />
        public ResponseObject<Portfolio> GetPortfolio()
        {
            var accountInfo = _binance.Client.GetAccountInfo(Configuration.Instance.BinanceClientSettings.ReceiveWindow);
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
    }
}