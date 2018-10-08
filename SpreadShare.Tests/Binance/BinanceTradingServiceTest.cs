using System;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.Tests.Binance
{
    /// <inheritdoc />
    public class BinanceTradingServiceTest : BaseTest
    {
        private readonly BinanceTradingService _tradingService;

        /// <summary>
        /// Constructor that fetches and starts the Binance Trading Service. This can
        /// be done for the scope of the entire class because all its members are read-only,
        /// rendering the service stateless and thus not violating any testing rules.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        public BinanceTradingServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _tradingService = (BinanceTradingService)serviceProvider.GetService<ITradingService>();
            _tradingService.Start();
        }
        

        /// <summary>
        /// Checks if the price of an asset is fetched and the result is reasonable.
        /// </summary>
        /// <param name="asset">Asset for which price should be fetched</param>
        [Theory]
        [InlineData("BNBETH")]
        [InlineData("VETETH")]
        [InlineData("IOTAETH")]
        [InlineData("ONTBTC")]
        public void GetPriceOfAsset(string asset)
        {         
            var query = _tradingService.GetCurrentPrice(CurrencyPair.Parse(asset));
            if (!query.Success) Assert.True(false, query.ToString());
            Logger.LogInformation($"The price of {asset} is {query.Data}");
            Assert.True(query.Data >= 0, $"Price is a non positive decimal: {query.Data}");
        }

        
        /// <summary>
        /// Checks if the performance of an asset can be fetched.
        /// </summary>
        /// <param name="hoursBack">Compare performance between now and a number of hours ago</param>
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetTopPerformance(int hoursBack)
        {
            if (hoursBack <= 0) {
                Assert.Throws<ArgumentException>(() => _tradingService.GetTopPerformance(hoursBack, DateTime.UtcNow));
                return;
            }
            var query = _tradingService.GetTopPerformance(hoursBack, DateTime.UtcNow);
            if (!query.Success) Assert.True(false, query.ToString());
            Logger.LogInformation($"Top performer is from the previous {hoursBack} hours is {query.Data.Item1} | ({query.Data.Item2*100-100}%)");
        }
    }
}