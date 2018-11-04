using System;
using System.Globalization;
using System.ServiceModel.Channels;
using Microsoft.Extensions.Logging;
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
        /// Check if the same currencies are in fact equal
        /// </summary>
        [Fact]
        public void SameCurrencyEquality()
        {
            var a = new Currency("ETH");
            var b = new Currency("ETH");

            Assert.True(a == b, $"Same currencies where not evaluated as equal");
            Assert.True(Equals(a, b), $"Same currencies where not evaluated as equal");
        }

        /// <summary>
        /// Check if different currencies are in fact unequal
        /// </summary>
        [Fact]
        public void DifferentCurrencyInequality()
        {
            var a = new Currency("BNB");
            var b = new Currency("VTHO");

            Assert.False(a == b, "Different currencies where evaluated as equal");
            Assert.False(Equals(a, b), "Different currencies where evaluated as equal");
        }

        /// <summary>
        /// Check if trying to create a currency with symbol 'null' throws.
        /// </summary>
        [Fact]
        public void NullCurrencyThrows()
        {
            Assert.Throws<ArgumentException>(() => new Currency(null));
            Assert.Throws<ArgumentException>(() => new Currency(string.Empty));
        }

        /// <summary>
        /// Tests if the serialized version of a currency is the correct JSON string
        /// </summary>
        /// <param name="input">input currency as string</param>
        [Theory]
        [InlineData("OMG")]
        [InlineData("eth")]
        [InlineData("lol")]
        [InlineData("DoGe")]
        public void JsonSerialization(string input)
        {
            Currency c = new Currency(input);
            string str = JsonConvert.SerializeObject(c);
            Assert.Equal(str, "\"" + input.ToUpper(CultureInfo.InvariantCulture) + "\"");
        }

        /// <summary>
        /// Tests if a JSON string is correctly deserialized to a Currency instance.
        /// </summary>
        /// <param name="input">input currency as string</param>
        [Theory]
        [InlineData("OMG")]
        [InlineData("eth")]
        [InlineData("lol")]
        [InlineData("DoGe")]
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
        [InlineData("OMG")]
        [InlineData("eth")]
        [InlineData("lol")]
        [InlineData("DoGe")]
        public void JsonParsingIsIdentity(string input)
        {
            var c = new Currency(input);
            var jsonstr = JsonConvert.SerializeObject(c);
            var post = JsonConvert.DeserializeObject<Currency>(jsonstr);
            Assert.Equal(c, post);
        }
    }
}