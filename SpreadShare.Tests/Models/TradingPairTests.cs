using System;
using System.Collections.Generic;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class TradingPairTests : BaseTest
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
        public void ParsingHappyFlow()
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

        [Fact]
        public void AddParseEntryNull()
        {
            var pair = GetTradingPair("BNB", "ETH");
            
            Assert.Throws<ArgumentNullException>(() => TradingPair.AddParseEntry("BNBETH", null));
            Assert.Throws<ArgumentNullException>(() => TradingPair.AddParseEntry(null, pair));
        }

        [Fact]
        public void AddParseEntryEmpty()
        {
            var pair = GetTradingPair("BNB", "ETH");

            Assert.Throws<ArgumentException>(() => TradingPair.AddParseEntry(String.Empty, pair));
            Assert.Throws<ArgumentException>(() => TradingPair.AddParseEntry(" ", pair));
        }

        [Fact]
        public void AddParseEntryOverwrites()
        {
            var pre = GetTradingPair("BNB", "ETH", 0);
            TradingPair.AddParseEntry("BNBETH", pre);

            var post = GetTradingPair("BNB", "ETH", 1);
            TradingPair.AddParseEntry("BNBETH", post);

            var postParse = TradingPair.Parse("BNBETH");
            Assert.NotEqual(pre.Decimals, postParse.Decimals);
        }

        [Fact]
        public void ParseFromStringNull()
        {
            Assert.Throws<ArgumentNullException>(() => TradingPair.Parse(null));
        }

        [Fact]
        public void ParseFromStringEmpty()
        {
            Assert.Throws<ArgumentException>(() => TradingPair.Parse(String.Empty));
            Assert.Throws<ArgumentException>(() => TradingPair.Parse(" "));
        }

        [Fact]
        public void ParseFromStringInvalid()
        {
            Assert.Throws<KeyNotFoundException>(() => TradingPair.Parse("ETHETH"));
        }

        [Fact]
        public void RoundingHappyFlow()
        {
            var t = GetTradingPair("BNB", "ETH", 3);
            decimal amount = 420.691234M;
            decimal corrected = Math.Floor(amount * 1000) / 1000;
            decimal calc = t.RoundToTradable(amount);
            Assert.Equal(corrected, calc);
        }

        private TradingPair GetTradingPair(string left, string right, int decimals = 0)
        {
            Currency cleft = new Currency(left);
            Currency cright = new Currency(right);
            return new TradingPair(cleft, cright, decimals);
        }
    }
}
