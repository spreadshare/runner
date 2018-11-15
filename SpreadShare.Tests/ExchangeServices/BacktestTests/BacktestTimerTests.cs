using System;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BacktestTests
{
    /// <summary>
    /// Collection of tests for the backtest timer provider.
    /// </summary>
    public class BacktestTimerTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTimerTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public BacktestTimerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Test if the the timer keeps track of its current time element
        /// </summary>
        /// <param name="minutes">Minutes to mock the timer for</param>
        [Theory]
        [InlineData(1000)]
        [InlineData(0)]
        [InlineData(69696969)]
        public void CurrentTimeIncrements(uint minutes)
        {
            var start = new DateTimeOffset(2018, 1, 1, 12, 42, 11, 500, TimeSpan.Zero);
            var timer = new BacktestTimerProvider(LoggerFactory, start);
            timer.SetTimer(minutes, () =>
            {
                var newTime = timer.CurrentTime;
                var checkTime = start + TimeSpan.FromMinutes(minutes);
                Assert.Equal(newTime.Second, checkTime.Second);
            });
        }
    }
}