using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    /// <summary>
    /// Tests the functionality of the portfolio fetcher service.
    /// </summary>
    public class PortfolioFetcherTests : BaseTest
    {
        private readonly IPortfolioFetcherService _fetcher;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public PortfolioFetcherTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _fetcher = _serviceProvider.GetService<IPortfolioFetcherService>();
        }

        /// <summary>
        /// Tests if the Binance portfolio is successfully fetched.
        /// </summary>
        [Fact]
        public void BinancePortfolioIsFetched()
        {
            // Connect the communications
            _serviceProvider.GetService<BinanceCommunicationsService>().Connect();
            var query = _fetcher.GetPortfolio(Exchange.Binance);
            Assert.True(query.Success);
            if (query.Success)
            {
                Logger.LogInformation(query.Data.ToJson());
            }
        }

        /// <summary>
        /// Tests if the backtest portfolio is successfully fetched.
        /// </summary>
        [Fact]
        public void BacktestPortfolioIsFetched()
        {
            // Connection the communications
            _serviceProvider.GetService<BacktestCommunicationService>().Connect();
            var query = _fetcher.GetPortfolio(Exchange.Backtesting);
            if (query.Success)
            {
                Logger.LogInformation(query.Data.ToJson());
            }
            else
            {
                Logger.LogError(query.Message);
            }

            Assert.True(query.Success);
        }
    }
}