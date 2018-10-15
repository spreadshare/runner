using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsService;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Binance
{
    /// <summary>
    /// Collection of tests for the Binance Trading Service
    /// </summary>
    public class BinanceTradingServiceTest : BaseTest
    {
        private readonly BinanceTradingService _tradingService;
        private readonly List<CurrencyPair> _pairs;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceTradingServiceTest"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        public BinanceTradingServiceTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _tradingService = (BinanceTradingService)serviceProvider.GetService<ITradingService>();
            _tradingService.Start();
            var settings = (SettingsService)serviceProvider.GetService<ISettingsService>();
            settings.Start();
            _pairs = new List<CurrencyPair>()
            {
                CurrencyPair.Parse("BNBETH"),
                CurrencyPair.Parse("ZRXETH"),
                CurrencyPair.Parse("PIVXBTC")
            };
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
            var query = _tradingService.GetCurrentPriceLastTrade(CurrencyPair.Parse(asset));
            if (!query.Success)
            {
                Assert.True(false, query.ToString());
            }

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
            if (hoursBack <= 0)
            {
                Assert.Throws<ArgumentException>(() => _tradingService.GetTopPerformance(_pairs, hoursBack, DateTime.UtcNow));
                return;
            }

            var query = _tradingService.GetTopPerformance(_pairs, hoursBack, DateTime.UtcNow);
            if (!query.Success)
            {
                Assert.True(false, query.ToString());
            }
        }

        /// <summary>
        /// Check it the highest bid of the order book is lower than the lowest ask.
        /// A violation of this invariant would mean a negative spread which is impossible.
        /// </summary>
        /// <param name="symbol">The symbol of the trading pair</param>
        [Theory]
        [InlineData("XRPETH")]
        [InlineData("ZRXBTC")]
        public void HighestBidIsLowerThanLowestAsk(string symbol)
        {
            CurrencyPair pair;
            try
            {
                pair = CurrencyPair.Parse(symbol);
            }
            catch
            {
                Assert.True(false, $"Symbol could not be parsed (invalid test data)");
                return;
            }

            var bidQuery = _tradingService.GetCurrentPriceTopBid(pair);
            var askQuery = _tradingService.GetCurrentPriceTopAsk(pair);
            if (!bidQuery.Success || !askQuery.Success)
            {
                Assert.True(false, $"Could not get data for tests. \n{bidQuery}\n{askQuery}");
            }

            Assert.True(bidQuery.Data < askQuery.Data, $"{bidQuery.Data} (highest bid) is higher than the lowest ask: {askQuery.Data}");
        }
    }
}