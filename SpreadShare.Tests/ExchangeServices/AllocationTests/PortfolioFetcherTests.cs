using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    public class PortfolioFetcherTests : BaseTest
    {
        public PortfolioFetcherTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void PortfolioIsFetched()
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var portfolio = serviceProvider.GetService<IPortfolioFetcherService>();
            var query = portfolio.GetPortfolio(Exchange.Binance);
            Assert.True(query.Success);
            if (query.Success)
            {
                Logger.LogInformation(query.Data.ToJson());
            }
        }
    }
}