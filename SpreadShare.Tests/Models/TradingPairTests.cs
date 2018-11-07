using System;
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
        [Fact]
        public void ConstructorHappyFlow()
        {
            var left = new Currency("BNB");
            var right = new Currency("ETH");
            int decimals = 0;
            var t = new TradingPair(left, right, decimals);
            Assert.Equal(left, t.Left);
            Assert.Equal(right, t.Right);
        }

        [Fact]
        public void ConstructorNull()
        {
            var c = new Currency("ETH");
            Assert.Throws<ArgumentNullException>(() => new TradingPair(null, c, 0));
            Assert.Throws<ArgumentNullException>(() => new TradingPair(c, null, 0));
        }

        [Fact]
        public void ConstructorEqualCurrencies()
        {
            var left = new Currency("ETH");
            var right = new Currency("ETH");
            int decimals = 0;
            Assert.Throws<ArgumentException>(() => new TradingPair(left, right, decimals));
        }

        [Fact]
        public void ConstructorNegativeDecimals()
        {
            var left = new Currency("BNB");
            var right = new Currency("ETH");
            int decimals = -1;
            Assert.Throws<ArgumentException>(() => new TradingPair(left, right, decimals));
        }

        [Fact]
        public void AddParseEntryHappyFlow()
        {
            var left = new Currency("BNB");
            var right = new Currency("ETH");
            int decimals = 0;
            var pair = new TradingPair(left, right, decimals);
            TradingPair.AddParseEntry("BNBETH", pair);
            var parsed = TradingPair.Parse("BNBETH");
            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
        }
}
