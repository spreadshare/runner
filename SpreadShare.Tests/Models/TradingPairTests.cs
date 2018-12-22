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
            int quantityDecimals = 1;
            int priceDecimals = 2;
            var pair = new TradingPair(left, right, quantityDecimals, priceDecimals);
            Assert.Equal(left, pair.Left);
            Assert.Equal(right, pair.Right);
            Assert.Equal(quantityDecimals, pair.QuantityDecimals);
            Assert.Equal(priceDecimals, pair.PriceDecimals);
        }

        [Fact]
        public void ConstructorNull()
        {
            var currency = new Currency("ETH");
            Assert.Throws<ArgumentNullException>(() => new TradingPair(null, currency, 0, 0));
            Assert.Throws<ArgumentNullException>(() => new TradingPair(currency, null, 0, 0));
        }

        [Fact]
        public void ConstructorEqualCurrencies()
        {
            var left = new Currency("ETH");
            var right = new Currency("ETH");
            Assert.Throws<ArgumentException>(() => new TradingPair(left, right, 0, 0));
        }

        [Fact]
        public void ConstructorNegativeDecimals()
        {
            var left = new Currency("BNB");
            var right = new Currency("ETH");
            Assert.Throws<ArgumentOutOfRangeException>(() => new TradingPair(left, right, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TradingPair(left, right, 0, -1));
        }

        [Fact]
        public void ParsingHappyFlow()
        {
            var pair = GetTradingPair("BNB", "ETH", 2);
            TradingPair.AddParseEntry("BNBETH", pair);
            var parsed = TradingPair.Parse("BNBETH");

            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.QuantityDecimals, parsed.QuantityDecimals);
            Assert.Equal(pair.PriceDecimals, parsed.PriceDecimals);
        }

        [Fact]
        public void ParsingWithWhitspace()
        {
            var pair = GetTradingPair("BNB", "ETH");
            TradingPair.AddParseEntry("BNBETH", pair);
            var parsed = TradingPair.Parse(" BNB ETH ");

            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.PriceDecimals, parsed.PriceDecimals);
        }

        [Fact]
        public void AddParseEntryWithWhiteSpace()
        {
            var pair = GetTradingPair("BNB", "ETH", 7);
            TradingPair.AddParseEntry(" BNB ETH ", pair);
            var parsed = TradingPair.Parse("BNBETH");

            Assert.Equal(pair.Left, parsed.Left);
            Assert.Equal(pair.Right, parsed.Right);
            Assert.Equal(pair.QuantityDecimals, parsed.QuantityDecimals);
            Assert.Equal(pair.PriceDecimals, parsed.PriceDecimals);
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

            Assert.Throws<ArgumentException>(() => TradingPair.AddParseEntry(string.Empty, pair));
            Assert.Throws<ArgumentException>(() => TradingPair.AddParseEntry(" ", pair));
        }

        [Fact]
        public void AddParseEntryOverwrites()
        {
            var pre = GetTradingPair("BNB", "ETH", 0, 0);
            TradingPair.AddParseEntry("BNBETH", pre);

            var post = GetTradingPair("BNB", "ETH", 1, 1);
            TradingPair.AddParseEntry("BNBETH", post);

            var postParse = TradingPair.Parse("BNBETH");
            Assert.NotEqual(pre.QuantityDecimals, postParse.QuantityDecimals);
            Assert.NotEqual(pre.PriceDecimals, postParse.PriceDecimals);
        }

        [Fact]
        public void ParseFromStringNull()
        {
            Assert.Throws<ArgumentNullException>(() => TradingPair.Parse(null));
        }

        [Fact]
        public void ParseFromStringEmpty()
        {
            Assert.Throws<ArgumentException>(() => TradingPair.Parse(string.Empty));
            Assert.Throws<ArgumentException>(() => TradingPair.Parse(" "));
        }

        [Fact]
        public void ParseFromStringInvalid()
        {
            Assert.Throws<KeyNotFoundException>(() => TradingPair.Parse("ETHETH"));
        }

        [Fact]
        public void ToStringCheck()
        {
            var pair = GetTradingPair("BNB", "ETH", 0);
            Assert.Equal("BNBETH", pair.ToString());
        }

        [Fact]
        public void RoundToTradableDecimalHappyFlow()
        {
            var pair = GetTradingPair("BNB", "ETH", 3);
            decimal amount = 420.691634M;
            decimal corrected = 420.691M;
            decimal calc = pair.RoundToTradable(amount);
            Assert.Equal(corrected, calc);
        }

        [Fact]
        public void RoundToTradableBalanceHappyFlow()
        {
            var pair = GetTradingPair("BNB", "ETH", 3);
            Balance balance = new Balance(new Currency("BNB"), 254.1234M, 0.0000000000000001M);
            Balance corrected = new Balance(new Currency("BNB"), 254.123M, 0);
            Balance calc = pair.RoundToTradable(balance);
            Assert.Equal(corrected.Free, calc.Free);
            Assert.Equal(corrected.Locked, calc.Locked);
        }

        [Fact]
        public void RoundToTradableNegativeNumbers()
        {
            var pair = GetTradingPair("BNB", "ETH", 3);
            Balance balance = new Balance(new Currency("BNB"), -1, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => pair.RoundToTradable(balance));
        }

        [Fact]
        public void RoundToPricableHappyFlow()
        {
            var pair = GetTradingPair("BNB", "ETH", 0, 4);
            decimal amount = 521.4923842M;
            decimal corrected = 521.4923M;
            decimal calc = pair.RoundToPriceable(amount);
            Assert.Equal(corrected, calc);
        }

        [Fact]
        public void RoundToPriceableNegativeNumber()
        {
            var pair = GetTradingPair("BNB", "ETH", 0, 2);
            var price = -1M;
            Assert.Throws<ArgumentOutOfRangeException>(() => pair.RoundToPriceable(price));
        }

        internal static TradingPair GetTradingPair(string strLeft, string strRight, int quantityDecimals = 0, int priceDecimals = 0)
        {
            Currency left = new Currency(strLeft);
            Currency right = new Currency(strRight);
            return new TradingPair(left, right, quantityDecimals, priceDecimals);
        }
    }
}
