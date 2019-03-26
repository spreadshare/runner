using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.ExchangeServices.ProvidersBinance;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    /// <summary>
    /// Tests the functionality of the portfolio fetcher service.
    /// </summary>
    public class PortfolioFetcherTests : BaseTest
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public PortfolioFetcherTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
        }

        /// <summary>
        /// Tests if the Binance portfolio is successfully fetched.
        /// </summary>
        [Fact]
        public void BinancePortfolioIsFetched()
        {
            // Connect the communications
            var binance = _serviceProvider.GetService<BinanceCommunicationsService>();
            binance.Connect();
            var fetcher = new BinancePortfolioFetcher(LoggerFactory, binance);
            var query = fetcher.GetPortfolio();
            Assert.True(query.Success);
        }

        /// <summary>
        /// Tests if the backtest portfolio is successfully fetched.
        /// </summary>
        [Fact]
        public void BacktestPortfolioIsFetched()
        {
            // Connection the communications
            var backtest = _serviceProvider.GetService<BacktestCommunicationService>();
            backtest.Connect();
            var fetcher = new BacktestPortfolioFetcher(LoggerFactory, backtest);
            var query = fetcher.GetPortfolio();
            if (!query.Success)
            {
                Logger.LogError(query.Message);
            }

            Assert.True(query.Success);
        }
    }
}