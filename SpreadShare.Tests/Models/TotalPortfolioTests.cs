using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpreadShare.Tests.Models
{
    public class TotalPortfolioTests : BaseTest
    {
        public TotalPortfolioTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var total = new TotalPortfolio();
            Assert.Empty(total.GetSummedChildren().AllBalances());
        }

        [Fact]
        public void GetAlgorithmAllocationHappyFlow()
        {
            var total = GetDefaultPortfolio();
            var algo = typeof(SimpleBandWagonAlgorithm);

            var allocation = total.GetAlgorithmAllocation(algo);
            Assert.NotNull(allocation);
            Assert.False(allocation.AllBalances().Count() == 0, "Algorithm allocation does have balances");
        }

        [Fact]
        public void GetAlgorithmAllocationNone()
        {
            var total = GetDefaultPortfolio();
            var algo = typeof(bool);

            var allocation = total.GetAlgorithmAllocation(algo);
            Assert.NotNull(allocation);
            Assert.True(!allocation.AllBalances().Any(), "Algorithm 'bool' should not be allocated");
        }

        [Fact]
        public void SetAllocationHappyFlow()
        {
            var total = new TotalPortfolio();
            Type algo1 = typeof(SimpleBandWagonAlgorithm);
            Type algo2 = typeof(bool); // Any type will do for now
            total.SetAlgorithmAllocation(algo1, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            total.SetAlgorithmAllocation(algo2, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 5, 40) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), -6, 32.00000000001M) }
            }));

            Assert.Equal(2.001M, total.GetAlgorithmAllocation(algo1).GetAllocation(new Currency("ETH")).Free);
            Assert.Equal(4, total.GetAlgorithmAllocation(algo1).GetAllocation(new Currency("ETH")).Locked);
            Assert.Equal(7, total.GetAlgorithmAllocation(algo1).GetAllocation(new Currency("BTC")).Free);
            Assert.Equal(1, total.GetAlgorithmAllocation(algo1).GetAllocation(new Currency("BTC")).Locked);

            Assert.Equal(5, total.GetAlgorithmAllocation(algo2).GetAllocation(new Currency("ETH")).Free);
            Assert.Equal(40, total.GetAlgorithmAllocation(algo2).GetAllocation(new Currency("ETH")).Locked);
            Assert.Equal(-6, total.GetAlgorithmAllocation(algo2).GetAllocation(new Currency("BTC")).Free);
            Assert.Equal(32.00000000001M, total.GetAlgorithmAllocation(algo2).GetAllocation(new Currency("BTC")).Locked);
        }

        [Fact]
        public void SetAllocationOverwrites()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(SimpleBandWagonAlgorithm);

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 5, 40) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), -6, 32.00000000001M) }
            }));

            Assert.Equal(5, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("ETH")).Free);
            Assert.Equal(40, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("ETH")).Locked);
            Assert.Equal(-6, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("BTC")).Free);
            Assert.Equal(32.00000000001M, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("BTC")).Locked);
        }

        [Fact]
        public void AllocationIsReportedHappyFlow()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(SimpleBandWagonAlgorithm);

            Assert.False(total.IsAllocated(algo));

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            Assert.True(total.IsAllocated(algo));
        }

        [Fact]
        public void AllocationIsReportedNoExist()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(SimpleBandWagonAlgorithm);

            Assert.False(total.IsAllocated(algo));

            total.SetAlgorithmAllocation(typeof(bool), new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            Assert.False(total.IsAllocated(algo));
        }

        [Fact]
        public void ChildrenAreSummed()
        {
            var total = GetDefaultPortfolio();
            Type algo = typeof(bool);

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.002M, 2345342) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), -4, 1) },
                { new Currency("BNB"), new Balance(new Currency("BNB"), -6, 9000.000000001M) }
            }));

            var summed = total.GetSummedChildren();

            Assert.Equal(4.003M, summed.GetAllocation(new Currency("ETH")).Free);
            Assert.Equal(2345346, summed.GetAllocation(new Currency("ETH")).Locked);
            Assert.Equal(3, summed.GetAllocation(new Currency("BTC")).Free);
            Assert.Equal(2, summed.GetAllocation(new Currency("BTC")).Locked);
            Assert.Equal(-6, summed.GetAllocation(new Currency("BNB")).Free);
            Assert.Equal(9000.000000001M, summed.GetAllocation(new Currency("BNB")).Locked);
        }

        private static TotalPortfolio GetDefaultPortfolio()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(SimpleBandWagonAlgorithm);

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            return total;
        }
    }
}