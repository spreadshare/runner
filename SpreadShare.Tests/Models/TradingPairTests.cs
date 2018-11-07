using System;
using System.Collections.Generic;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class TradingPairTests : BaseTest
    {
        public TradingPairTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var left = new Currency("BNB");
            var right = new Currency("ETH");
            int decimals = 0;
            var pair = new TradingPair(left, right, decimals);
            Assert.Equal(left, pair.Left);
            Assert.Equal(right, pair.Right);
            Assert.Equal(decimals, pair.Decimals);
        }

        [Fact]
        public void ConstructorNull()
        {
            var currency = new Currency("ETH");
            Assert.Throws<ArgumentNullException>(() => new TradingPair(null, currency, 0));
            Assert.Throws<ArgumentNullException>(() => new TradingPair(currency, null, 0));
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
            var pair = GetTradingPair("BNB", "ETH", 2);
            TradingPair.AddParseEntry("BNBETH", pair);
            var parsed = TradingPair.Parse("BNBETH");

            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.Decimals, parsed.Decimals);
        }

        [Fact]
        public void ParsingWithWhitspace()
        {
            var pair = GetTradingPair("BNB", "ETH");
            TradingPair.AddParseEntry("BNBETH", pair);
            var parsed = TradingPair.Parse(" BNB ETH ");
            
            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.Decimals, parsed.Decimals);
        }

        [Fact]
        public void AddParseEntryWithWhiteSpace()
        {
            var pair = GetTradingPair("BNB", "ETH", 7);
            TradingPair.AddParseEntry(" BNB ETH ", pair);
            var parsed = TradingPair.Parse("BNBETH");
            
            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.Decimals, parsed.Decimals);
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
            var pair = GetTradingPair("BNB", "ETH", 3);
            decimal amount = 420.691234M;
            decimal corrected = Math.Floor(amount * 1000) / 1000;
            decimal calc = pair.RoundToTradable(amount);
            Assert.Equal(corrected, calc);
        }

        private TradingPair GetTradingPair(string strLeft, string strRight, int decimals = 0)
        {
            Currency left = new Currency(strLeft);
            Currency right = new Currency(strRight);
            return new TradingPair(left, right, decimals);
        }
    }
}
