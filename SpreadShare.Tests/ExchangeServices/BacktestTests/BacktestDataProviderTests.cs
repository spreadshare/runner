using System;
using SpreadShare.ExchangeServices.Backtesting;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BacktestTests
{
    public class BacktestDataProviderTests : BaseTest
    {
        public BacktestDataProviderTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void TestLol()
        {
            var timer = new BacktestTimerProvider(LoggerFactory, DateTimeOffset.Now);
            var data = new BacktestDataProvider(LoggerFactory, timer);
        }
    }
}