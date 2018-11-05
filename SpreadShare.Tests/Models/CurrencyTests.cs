using System;
using System.Globalization;
using Newtonsoft.Json;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    /// <summary>
    /// Tests regarding currencies
    /// </summary>
    public class CurrencyTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public CurrencyTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Constructor should create Currency
        /// </summary>
        /// <param name="symbol">Symbol of currency</param>
        [Theory]
        [InlineData("ETH")]
        [InlineData("eth")]
        public void ConstructorHappyFlow(string symbol)
        {
            Currency a = new Currency(symbol);
            Assert.False(a is null, "Currency constructor threw exceptions or failed to initialize?");
        }

        /// <summary>
        /// Constructor should not accept null.
        /// </summary>
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentException>(() => new Currency(null));
        }

        /// <summary>
        /// Constructor should not accept empty strings.
        /// </summary>
        [Fact]
        public void ConstructorEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Currency(string.Empty));
            Assert.Throws<ArgumentException>(() => new Currency(" "));
        }

        /// <summary>
        /// Currency.ToString() should report the symbol of the currency.
        /// </summary>
        [Fact]
        public void ToStringHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Assert.True(a.ToString() == symbolUppercase, "Currency.ToString() did not return the correct symbol");
        }

        /// <summary>
        /// Currency.ToString() should report the symbol of the currency in uppercase and trimmed.
        /// </summary>
        [Fact]
        public void ToStringCaseSensitiveUntrimmed()
        {
            const string symbolUppercase = "eth ";
            Currency a = new Currency(symbolUppercase);

            // Check if symbol is in uppercase
            Assert.True(
                a.ToString() == a.ToString().ToUpperInvariant(),
                "Currency.Symbol did not return the symbol in uppercase");

            // Check if symbol is trimmed
            Assert.True(
                a.ToString() == a.ToString().Trim(),
                "Currency.ToString() did not return the symbol trimmed of whitespace");

            // Check if symbol is the correct symbol
            Assert.True(
                a.ToString() == symbolUppercase.ToUpperInvariant().Trim(),
                "Currency.ToString() did not return the correct symbol");
        }

        /// <summary>
        /// Currency.Symbol() should report the symbol of the currency.
        /// </summary>
        [Fact]
        public void GetSymbolHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Assert.True(a.Symbol == symbolUppercase, "Currency.Symbol did not return the correct symbol");
        }

        /// <summary>
        /// Currency.Symbol() should report the symbol of the currency in uppercase and trimmed.
        /// </summary>
        [Fact]
        public void GetSymbolCaseSensitiveUntrimmed()
        {
            const string symbolUppercase = "eth ";
            Currency a = new Currency(symbolUppercase);

            // Check if symbol is in uppercase
            Assert.True(
                a.Symbol == a.Symbol.ToUpperInvariant(),
                "Currency.Symbol did not return the symbol in uppercase");

            // Check if symbol is trimmed
            Assert.True(
                a.Symbol == a.Symbol.Trim(),
                "Currency.Symbol did not return the symbol trimmed of whitespace");

            // Check if symbol is the correct symbol
            Assert.True(
                a.Symbol == symbolUppercase.ToUpperInvariant().Trim(),
                "Currency.Symbol did not return the correct symbol");
        }

        /// <summary>
        /// Equals method should report True if both symbols are the same.
        /// </summary>
        [Fact]
        public void EqualsHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolUppercase);
            Assert.True(a.Equals(b), "Currency.Equals(Currency) failed for the same tickers");
        }

        /// <summary>
        /// Equals method should report True if both symbols are the same, although in different casing.
        /// </summary>
        [Fact]
        public void EqualsDifferentCasing()
        {
            const string symbolUppercase = "ETH";
            const string symbolLowercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolLowercase);
            Assert.True(a.Equals(b), "Currency.Equals(Currency) failed for the same tickers");
        }

        /// <summary>
        /// Equals method should report False for different symbols.
        /// </summary>
        [Fact]
        public void EqualsDifferentCurrencies()
        {
            const string symbolUppercase = "ETH";
            const string notEth = "NOT_ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(notEth);

            Assert.False(a.Equals(b), "Currency.Equals(Currency) should report False for different tickers");
        }

        /// <summary>
        /// Equals method should report False if one currency is instantiated and the other is null
        /// </summary>
        [Fact]
        public void EqualsNullEqualityAndCurrency()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = null;
            Assert.False(a.Equals(b), "Currency.Equals(null) should report False and not throw any errors");
        }

        /// <summary>
        /// Currency.GetHashCode() should report True if both symbols are the same.
        /// </summary>
        [Fact]
        public void GetHashCodeHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolUppercase);
            Assert.True(a.GetHashCode() == b.GetHashCode(), "c1.GetHashCode() == c2.GetHashCode() failed for the same tickers");
        }

        /// <summary>
        /// Currency.GetHashCode() should report True if both symbols are the same, although in different casing.
        /// </summary>
        [Fact]
        public void GetHashCodeDifferentCasing()
        {
            const string symbolUppercase = "ETH";
            const string symbolLowercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolLowercase);
            Assert.True(a.GetHashCode() == b.GetHashCode(), "c1.GetHashCode() == c2.GetHashCode() failed for the same tickers");
        }

        /// <summary>
        /// Currency.GetHashCode() should report False for different symbols.
        /// </summary>
        [Fact]
        public void GetHashCodeDifferentCurrencies()
        {
            const string symbolUppercase = "ETH";
            const string notEth = "NOT_ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(notEth);

            Assert.False(a.GetHashCode() == b.GetHashCode(), "c1.GetHashCode() == c2.GetHashCode() should report False for different tickers");
        }

        /// <summary>
        /// == operator should report True if both symbols are the same.
        /// </summary>
        [Fact]
        public void EqualOperatorHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolUppercase);
            Assert.True(a == b, "Currency == Currency failed for the same tickers");
        }

        /// <summary>
        /// == operator should report True if both symbols are the same, although in different casing.
        /// </summary>
        [Fact]
        public void EqualOperatorDifferentCasing()
        {
            const string symbolUppercase = "ETH";
            const string symbolLowercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolLowercase);
            Assert.True(a == b, "Currency == Currency failed for the same tickers");
        }

        /// <summary>
        /// == operator should report False for different symbols.
        /// </summary>
        [Fact]
        public void EqualOperatorDifferentCurrencies()
        {
            const string symbolUppercase = "ETH";
            const string notEth = "NOT_ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(notEth);

            Assert.False(a == b, "Currency == Currency should report False for different tickers");
        }

        /// <summary>
        /// == operator should report True if both currencies are null.
        /// </summary>
        [Fact]
        public void EqualOperatorNullEquality()
        {
            Currency a = null;
            Currency b = null;
            Assert.True(a == b, "null == null should report True and not throw any errors");
        }

        /// <summary>
        /// == operator should report False if one currency is instantiated and the other is null
        /// </summary>
        [Fact]
        public void EqualOperatorNullEqualityAndCurrency()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = null;
            Assert.False(a == b, "Currency == null should report False and not throw any errors");
        }

        /// <summary>
        /// != operator should report False if both symbols are the same.
        /// </summary>
        [Fact]
        public void UnequalOperatorHappyFlow()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolUppercase);
            Assert.False(a != b, "Currency != Currency should report False for same symbols");
        }

        /// <summary>
        /// != operator should report False if both symbols are the same, although in different casing.
        /// </summary>
        [Fact]
        public void UnequalOperatorDifferentCasing()
        {
            const string symbolUppercase = "ETH";
            const string symbolLowercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(symbolLowercase);
            Assert.False(a != b, "Currency != Currency should report False for same symbols");
        }

        /// <summary>
        /// != operator should report True for different symbols.
        /// </summary>
        [Fact]
        public void UnequalOperatorDifferentCurrencies()
        {
            const string symbolUppercase = "ETH";
            const string notEth = "NOT_ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = new Currency(notEth);

            Assert.True(a != b, "Currency != Currency should report True for different tickers");
        }

        /// <summary>
        /// != operator should report False if both currencies are null.
        /// </summary>
        [Fact]
        public void UnequalOperatorNullEquality()
        {
            Currency a = null;
            Currency b = null;
            Assert.False(a != b, "null == null should report False and not throw any errors");
        }

        /// <summary>
        /// != operator should report True if one currency is instantiated and the other is null.
        /// </summary>
        [Fact]
        public void UnequalOperatorNullEqualityAndCurrency()
        {
            const string symbolUppercase = "ETH";
            Currency a = new Currency(symbolUppercase);
            Currency b = null;
            Assert.True(a != b, "Currency != null should report True and not throw any errors");
        }

        /// <summary>
        /// Tests if the serialized version of a currency is the correct JSON string
        /// </summary>
        /// <param name="input">input currency as string</param>
        [Theory]
        [InlineData("ETH")]
        [InlineData("eth")]
        [InlineData("eth ")]
        public void JsonSerialization(string input)
        {
            Currency c = new Currency(input);
            string str = JsonConvert.SerializeObject(c);
            Assert.Equal(str, "\"" + input.ToUpperInvariant().Trim() + "\"");
        }

        /// <summary>
        /// Tests if a JSON string is correctly deserialized to a Currency instance.
        /// </summary>
        /// <param name="input">input currency as string</param>
        [Theory]
        [InlineData("ETH")]
        [InlineData("eth")]
        public void JsonDeserialization(string input)
        {
            var c = JsonConvert.DeserializeObject<Currency>("\"" + input + "\"");
            Assert.Equal(input.ToUpper(CultureInfo.InvariantCulture), c.Symbol);
        }

        /// <summary>
        /// Tests if the combined operation of serialization and deserialization results in an identity function.
        /// </summary>
        /// <param name="input">input currency as string    </param>
        [Theory]
        [InlineData("ETH")]
        [InlineData("eth")]
        [InlineData("eth ")]
        public void JsonParsingIsIdentity(string input)
        {
            var c = new Currency(input);
            var jsonstr = JsonConvert.SerializeObject(c);
            var post = JsonConvert.DeserializeObject<Currency>(jsonstr);
            Assert.Equal(c, post);
        }
    }
}