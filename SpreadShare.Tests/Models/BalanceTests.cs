using System;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class BalanceTests : BaseTest
    {
        public BalanceTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, 0)]
        [InlineData(123352.343243523, 0.00000058)]
        public void ConstructorHappyFlow(decimal free, decimal locked)
        {
            var currency = new Currency("ETH");
            var balance = new Balance(currency, free, locked);

            Assert.Equal(free, balance.Free);
            Assert.Equal(locked, balance.Locked);
            Assert.Equal(currency, balance.Symbol);
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Balance(null, 0, 0));
        }

        [Fact]
        public void MonoidConstructor()
        {
            var currency = new Currency("ETH");
            var balance = Balance.Empty(currency);

            Assert.Equal(currency, balance.Symbol);
            Assert.Equal(0, balance.Free);
            Assert.Equal(0, balance.Locked);
        }

        [Fact]
        public void MonoidConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => Balance.Empty(null));
        }

        [Fact]
        public void IsValueType()
        {
            // Balance should be pass by value
            Type type = typeof(Balance);
            Assert.True(type.IsValueType && !type.IsPrimitive, "Balance should be pass by value");
        }

        [Theory]
        [InlineData(0, 0, 4, 2)]
        [InlineData(0.000000234, 14343323.001, 66653, 0.00000000003)]
        [InlineData(-324, -23, 24, -623.8)]
        public void BalanceSubtraction(decimal free1, decimal locked1, decimal free2, decimal locked2)
        {
            Balance left = new Balance(new Currency("ETH"), free1, locked1);
            Balance right = new Balance(new Currency("ETH"), free2, locked2);
            Balance sub = left - right;

            Assert.Equal(free1 - free2, sub.Free);
            Assert.Equal(locked1 - locked2, sub.Locked);
            Assert.Equal(left.Symbol, sub.Symbol);
        }

        [Theory]
        [InlineData(0, 0, 4, 2)]
        [InlineData(0.000000234, 14343323.001, 66653, 0.00000000003)]
        [InlineData(-324, -23, 24, -623.8)]
        public void BalanceAddition(decimal free1, decimal locked1, decimal free2, decimal locked2)
        {
            Balance left = new Balance(new Currency("ETH"), free1, locked1);
            Balance right = new Balance(new Currency("ETH"), free2, locked2);
            Balance add = left + right;

            Assert.Equal(free1 + free2, add.Free);
            Assert.Equal(locked1 + locked2, add.Locked);
            Assert.Equal(left.Symbol, add.Symbol);
        }

        [Fact]
        public void DifferentCurrencyOperations()
        {
            Balance left = new Balance(new Currency("ETH"), 0, 0);
            Balance right = new Balance(new Currency("BTC"), 0, 0);
            Assert.Throws<InvalidOperationException>(() => left + right);
            Assert.Throws<InvalidOperationException>(() => left - right);
        }

        [Fact]
        public void ToStringTest()
        {
            Balance balance = new Balance(new Currency("ETH"), 3, 5.5M);
            Assert.Equal("ETH -> 3|5.5", balance.ToString());
        }
    }
}