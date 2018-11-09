using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    /// <summary>
    /// Tests the functionality of the portfolio fetcher service
    /// </summary>
    public class PortfolioFetcherTests : BaseTest
    {
        private readonly IPortfolioFetcherService _fetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioFetcherTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public PortfolioFetcherTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _fetcher = serviceProvider.GetService<IPortfolioFetcherService>();
        }

        /// <summary>
        /// Tests if the Binance portfolio is successfully fetched.
        /// </summary>
        [Fact]
        public void BinancePortfolioIsFetched()
        {
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
            var query = _fetcher.GetPortfolio(Exchange.Backtesting);
            Assert.True(query.Success);
            if (query.Success)
            {
                Logger.LogInformation(query.Data.ToJson());
            }
        }
    }
}