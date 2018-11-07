using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    class TradingPairTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradingPairTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public TradingPairTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }


        /// <summary>
        /// Constructor should create Currency
        /// </summary>
        /// <param name="symbol">Symbol of currency</param>
        [Fact]
        public void ConstructorHappyFlow(string symbol)
        {
            Currency a = new Currency(symbol);
            Assert.False(a is null, "Currency constructor threw exceptions or failed to initialize?");
        }


    }
}
