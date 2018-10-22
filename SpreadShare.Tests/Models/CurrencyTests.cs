using System;
using SpreadShare.Models;
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
        }
    }
}