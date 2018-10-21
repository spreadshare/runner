using System;
using SpreadShare.Models;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class CurrencyTests : BaseTest
    {
        public CurrencyTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void SameCurrencyEquality()
        {
            var a = new Currency("ETH");
            var b = new Currency("ETH");
            
            Assert.True(a == b, $"Same currencies where not evaluated as equal");
        }

        [Fact]
        public void DifferentCurrencyInequality()
        {
            var a = new Currency("BNB");
            var b = new Currency("VTHO");
            
            Assert.False(a == b, "Different currencies where evaluated as equal");
        }

        [Fact]
        public void NullCurrencyThrows()
        {
            Assert.Throws<ArgumentException>(() => new Currency(null));
        }
    }
}