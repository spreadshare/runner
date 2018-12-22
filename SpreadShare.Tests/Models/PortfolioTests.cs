using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    /// <summary>
    /// Test collection for the portfolio model.
    /// </summary>
    public class PortfolioTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortfolioTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Generates output.</param>
        public PortfolioTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests valid parsing of values after the constructor.
        /// </summary>
        [Fact]
        public void ConstructorHappyFlow()
        {
            Currency c = new Currency("ETH");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, 1.0M, 0.0M) },
            });

            Assert.Equal(1.0M, portfolio.GetAllocation(new Currency("ETH")).Free);
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Portfolio(null));
        }

        [Fact]
        public void GetAllocationHappyFlow()
        {
            Currency c = new Currency("ETH");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>
            {
                { c, new Balance(c, 4, 0.0001M) },
            });

            Assert.Equal(4, portfolio.GetAllocation(c).Free);
            Assert.Equal(0.0001M, portfolio.GetAllocation(c).Locked);
        }

        [Fact]
        public void GetAllocationNull()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Throws<ArgumentNullException>(() => portfolio.GetAllocation(null));
        }

        /// <summary>
        /// Tests a number of cases to see of adding to portfolio adds the balances correctly.
        /// </summary>
        /// <param name="currency">Currency.</param>
        /// <param name="free1">Free balance for the first.</param>
        /// <param name="locked1">Locked balance for the first.</param>
        /// <param name="free2">Free balance for the second.</param>
        /// <param name="locked2">Locked balance for the second.</param>
        [Theory]
        [InlineData("ETH", 1.0, 0.0, 3.0, 5.0)]
        [InlineData("BTC", 0.000002, -4, 9943043.234, -343.232)]
        [InlineData("VET", 1.00000000001, 100000, 3.9999999, 1000000)]
        public void BalancesAreSummedHappyFlow(string currency, decimal free1, decimal locked1, decimal free2, decimal locked2)
        {
            Currency c = new Currency(currency);
            var first = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, free1, locked1) },
            });

            var second = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, free2, locked2) },
            });

            var result = Portfolio.Add(first, second);
            Assert.Equal(free1 + free2, result.GetAllocation(c).Free);
            Assert.Equal(locked1 + locked2, result.GetAllocation(c).Locked);
            Assert.Equal(free1, first.GetAllocation(c).Free);
            Assert.Equal(locked1, first.GetAllocation(c).Locked);
            Assert.Equal(free2, second.GetAllocation(c).Free);
            Assert.Equal(locked2, second.GetAllocation(c).Locked);
        }

        [Fact]
        public void BalancesAreSummedPartialOverlap()
        {
            Currency c1 = new Currency("ETH");
            Currency c2 = new Currency("BTC");
            Currency c3 = new Currency("DOGE");

            var first = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 4.0M, 1M) },
                { c2, new Balance(c2, 1.0M, 5.5M) },
            });

            var second = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 0.02M, 0) },
                { c3, new Balance(c3, 66.5M, 0.0000000004M) },
            });

            var result = Portfolio.Add(first, second);

            Assert.Equal(4.02M, result.GetAllocation(c1).Free);
            Assert.Equal(1.0M, result.GetAllocation(c1).Locked);
            Assert.Equal(1.0M, result.GetAllocation(c2).Free);
            Assert.Equal(5.5M, result.GetAllocation(c2).Locked);
            Assert.Equal(66.5M, result.GetAllocation(c3).Free);
            Assert.Equal(0.0000000004M, result.GetAllocation(c3).Locked);
        }

        [Fact]
        public void BalancesAreSummedNoOverlap()
        {
            Currency c1 = new Currency("ETH");
            Currency c2 = new Currency("BTC");

            var first = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 4.0M, 1M) },
            });

            var second = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c2, new Balance(c2, 1.0M, 5.5M) },
            });

            var result = Portfolio.Add(first, second);

            Assert.Equal(4.0M, result.GetAllocation(c1).Free);
            Assert.Equal(1.0M, result.GetAllocation(c1).Locked);
            Assert.Equal(1.0M, result.GetAllocation(c2).Free);
            Assert.Equal(5.5M, result.GetAllocation(c2).Locked);
        }

        [Fact]
        public void BalancesAreSummedNull()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Throws<ArgumentNullException>(() => Portfolio.Add(portfolio, null));
            Assert.Throws<ArgumentNullException>(() => Portfolio.Add(null, portfolio));
        }

        [Fact]
        public void GetListOfBalances()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("VET"), new Balance(new Currency("VET"), 0, 0) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 0, 0) },
            });

            var balances = portfolio.AllBalances().ToList();
            Assert.Equal(2, balances.Count);
            Assert.Contains(new Balance(new Currency("VET"), 0, 0), balances);
            Assert.Contains(new Balance(new Currency("BTC"), 0, 0), balances);
        }

        [Fact]
        public void GetListOfBalancesEmpty()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Empty(portfolio.AllBalances());
        }

        /// <summary>
        /// Tests if a trade execution is Processed correctly.
        /// </summary>
        [Fact]
        public void TradeIsProcessed()
        {
            Currency c1 = new Currency("BTC");
            Currency c2 = new Currency("ETH");
            var trade = new TradeExecution(
                new Balance(c1, 2.0M, 0.0M),
                new Balance(c2, 11.2422359M, 0.0M));

            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 2.5M, 0.0M) },
            });

            portfolio.UpdateAllocation(trade);

            Assert.Equal(0.5M, portfolio.GetAllocation(c1).Free);
            Assert.Equal(11.2422359M, portfolio.GetAllocation(c2).Free);
        }

        [Fact]
        public void TradeIsProcessedFreeToLocked()
        {
            Currency c1 = new Currency("BTC");
            var trade = new TradeExecution(
                new Balance(c1, 2.0M, 0.0M),
                new Balance(c1, 0.0M, 2.0M));

            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 2.5M, 0.0M) },
            });

            portfolio.UpdateAllocation(trade);

            Assert.Equal(0.5M, portfolio.GetAllocation(c1).Free);
            Assert.Equal(2.0M, portfolio.GetAllocation(c1).Locked);
        }

        [Fact]
        public void TradeIsProcessedZeroQuantityNewCurrency()
        {
            Currency c1 = new Currency("BTC");
            Currency c2 = new Currency("ETH");
            var trade = new TradeExecution(
                new Balance(c1, 0, 0),
                new Balance(c2, 0, 0));

            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());

            portfolio.UpdateAllocation(trade);
            Assert.Equal(0M, portfolio.GetAllocation(c1).Free);
            Assert.Equal(0M, portfolio.GetAllocation(c1).Locked);
            Assert.Equal(0M, portfolio.GetAllocation(c2).Free);
            Assert.Equal(0M, portfolio.GetAllocation(c2).Locked);
        }

        [Fact]
        public void TradeIsProcessedNull()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Throws<ArgumentNullException>(() => portfolio.UpdateAllocation(null));
        }

        [Fact]
        public void DuplicateWithScaleHappyFlow()
        {
            Currency c1 = new Currency("BTC");
            Currency c2 = new Currency("ETH");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>
            {
                { c1, new Balance(c1, 4, 8) },
                { c2, new Balance(c2, 5, 0.001M) },
            });

            var scaled = Portfolio.DuplicateWithScale(portfolio, 0.7M);

            Assert.Equal(2.8M, scaled.GetAllocation(c1).Free);
            Assert.Equal(5.6M, scaled.GetAllocation(c1).Locked);
            Assert.Equal(3.5M, scaled.GetAllocation(c2).Free);
            Assert.Equal(0.0007M, scaled.GetAllocation(c2).Locked);
        }

        [Fact]
        public void DuplicateWithScaleExactlyOne()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            var scaled = Portfolio.DuplicateWithScale(portfolio, 1);
            Assert.True(!scaled.AllBalances().Any());
        }

        [Fact]
        public void DuplicateWithScaleInvalidScale()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Throws<ArgumentException>(() => Portfolio.DuplicateWithScale(portfolio, -1));
        }

        [Fact]
        public void DuplicateWithScaleNull()
        {
            Assert.Throws<ArgumentNullException>(() => Portfolio.DuplicateWithScale(null, 1));
        }

        /// <summary>
        /// Tests if the difference between to portfolios is correct.
        /// </summary>
        [Fact]
        public void BalancesAreSubtractedHappyFlow()
        {
            Currency c1 = new Currency("BTC");
            Currency c2 = new Currency("ETH");
            Currency c3 = new Currency("VET");
            Currency c4 = new Currency("DOGE");
            var first = new Portfolio(new Dictionary<Currency, Balance>
            {
                { c1, new Balance(c1, 0.4M, 10M) },
                { c2, new Balance(c2, -0.45M, 0) },
                { c3, new Balance(c3, 0.66M, 1.0M) },
            });

            var second = new Portfolio(new Dictionary<Currency, Balance>
            {
                { c1, new Balance(c2, 10M, 10M) },
                { c2, new Balance(c2, 0.6M, 0M) },
                { c4, new Balance(c4, 4.2M, -0.00000001M) },
            });

            var diff = Portfolio.SubtractedDifferences(first, second);
            Assert.Equal(4, diff.Count);

            foreach (var balance in diff)
            {
                switch (balance.Symbol.ToString())
                {
                     case "BTC":
                         Assert.Equal(-9.6M, balance.Free);
                         Assert.Equal(0.0M, balance.Locked);
                         break;
                     case "ETH":
                         Assert.Equal(-1.05M, balance.Free);
                         Assert.Equal(0.0M, balance.Locked);
                         break;
                     case "VET":
                         Assert.Equal(0.66M, balance.Free);
                         Assert.Equal(1.0m, balance.Locked);
                         break;
                     case "DOGE":
                         Assert.Equal(-4.2M, balance.Free);
                         Assert.Equal(0.00000001M, balance.Locked);
                         break;
                }
            }
        }

        [Fact]
        public void BalancesAreSubtractedNull()
        {
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>());
            Assert.Throws<ArgumentNullException>(() => Portfolio.SubtractedDifferences(portfolio, null));
            Assert.Throws<ArgumentNullException>(() => Portfolio.SubtractedDifferences(null, portfolio));
        }

        [Fact]
        public void JsonString()
        {
            Currency c1 = new Currency("ETH");
            Currency c2 = new Currency("BTC");
            Currency c3 = new Currency("VET");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c1, new Balance(c1, 1.0M, 2.0M) },
                { c2, new Balance(c2, 99.0M, 0.0M) },
                { c3, new Balance(c3, 0.0M, 0.0M) },
            });

            string str = portfolio.ToJson();
            Assert.Contains("\"ETH\"", str, StringComparison.Ordinal);
            Assert.Contains("\"BTC\"", str, StringComparison.Ordinal);
            Assert.DoesNotContain("\"VET\"", str, StringComparison.Ordinal);
            Assert.Contains("\"Free\"", str, StringComparison.Ordinal);
            Assert.Contains("\"Locked\"", str, StringComparison.Ordinal);
            Assert.Contains(".", str, StringComparison.Ordinal);
        }
    }
}