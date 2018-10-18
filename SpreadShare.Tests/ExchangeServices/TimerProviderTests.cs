using System;
using SpreadShare.ExchangeServices.Provider;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices
{
    /// <inheritdoc />
    /// <summary>
    /// Tests regadring the timer provider.
    /// </summary>
    public class TimerProviderTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">used to create output</param>
        public TimerProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Test if the timer provides generates an argument exception if the given callback is null.
        /// </summary>
        [Fact]
        public void NoCallbackThrows()
        {
            var t = new ExchangeTimerProvider();
            Assert.Throws<ArgumentException>(() => t.SetTimer(0, null));
        }
    }
}