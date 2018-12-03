using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;

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

            var algo = typeof(TemplateAlgorithm);

            var allocation = total.GetAlgorithmAllocation(algo);
            Assert.NotNull(allocation);
            Assert.False(!allocation.AllBalances().Any(), "Algorithm allocation does have balances");
        }

        [Fact]
        public void GetAlgorithmAllocationNull()
        {
            var total = new TotalPortfolio();
            Assert.Throws<ArgumentNullException>(() => total.GetAlgorithmAllocation(null));
        }

        [Fact]
        public void GetAlgorithmAllocationInvalid()
        {
            var total = new TotalPortfolio();
            Assert.Throws<ArgumentException>(() => total.GetAlgorithmAllocation(typeof(bool)));
        }

        [Fact]
        public void GetAlgorithmAllocationNone()
        {
            var total = GetDefaultPortfolio();
            var algo = typeof(OtherAlgorithm);

            var allocation = total.GetAlgorithmAllocation(algo);
            Assert.NotNull(allocation);
            Assert.True(!allocation.AllBalances().Any(), "Algorithm 'OtherAlgorithm' should not be allocated");
        }

        [Fact]
        public void SetAllocationHappyFlow()
        {
            var total = new TotalPortfolio();
            Type algo1 = typeof(TemplateAlgorithm);
            Type algo2 = typeof(OtherAlgorithm);
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
        public void SetAllocationNull()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(TemplateAlgorithm);

            Assert.Throws<ArgumentNullException>(() => total.SetAlgorithmAllocation(
                null,
                new Portfolio(new Dictionary<Currency, Balance>())));

            Assert.Throws<ArgumentNullException>(() => total.SetAlgorithmAllocation(algo, null));
        }

        [Fact]
        public void SetAllocationInvalid()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(bool);

            Assert.Throws<ArgumentException>(() =>
                total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>())));
        }

        [Fact]
        public void SetAllocationOverwrites()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(TemplateAlgorithm);

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
        public void IsAllocatedHappyFlow()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(TemplateAlgorithm);

            Assert.False(total.IsAllocated(algo));

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            Assert.True(total.IsAllocated(algo));
        }

        [Fact]
        public void IsAllocatedNonExistent()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(OtherAlgorithm);

            Assert.False(total.IsAllocated(algo));

            total.SetAlgorithmAllocation(typeof(TemplateAlgorithm), new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            Assert.False(total.IsAllocated(algo));
        }

        [Fact]
        public void TradeIsProcessedHappyFlow()
        {
            var total = GetDefaultPortfolio();
            Type algo = typeof(TemplateAlgorithm);
            var trade = new TradeExecution(
                new Balance(new Currency("ETH"), 2, 0),
                new Balance(new Currency("VET"), 100, 0));

            total.ApplyTradeExecution(algo, trade);

            Assert.Equal(0.001M, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("ETH")).Free);
            Assert.Equal(100, total.GetAlgorithmAllocation(algo).GetAllocation(new Currency("VET")).Free);
        }

        [Fact]
        public void TradeIsProcessedNull()
        {
            var total = GetDefaultPortfolio();
            Type algo = typeof(TemplateAlgorithm);
            var trade = new TradeExecution(
                new Balance(new Currency("ETH"), 2, 0),
                new Balance(new Currency("VET"), 100, 0));

            Assert.Throws<ArgumentNullException>(() => total.ApplyTradeExecution(algo, null));
            Assert.Throws<ArgumentNullException>(() => total.ApplyTradeExecution(null, trade));
        }

        [Fact]
        public void TradeIsProcessedInvalidAlgorithm()
        {
            var total = new TotalPortfolio();
            Type algo = typeof(bool);
            var trade = new TradeExecution(
                new Balance(new Currency("ETH"), 2, 0),
                new Balance(new Currency("VET"), 100, 0));

            Assert.Throws<ArgumentException>(() => total.ApplyTradeExecution(algo, trade));
        }

        [Fact]
        public void ChildrenAreSummed()
        {
            var total = GetDefaultPortfolio();
            Type algo = typeof(OtherAlgorithm);

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
            Type algo = typeof(TemplateAlgorithm);

            total.SetAlgorithmAllocation(algo, new Portfolio(new Dictionary<Currency, Balance>
            {
                { new Currency("ETH"), new Balance(new Currency("ETH"), 2.001M, 4) },
                { new Currency("BTC"), new Balance(new Currency("BTC"), 7, 1) }
            }));

            return total;
        }

        // Disable warning about classes that are not instantiated
        #pragma warning disable CA1812
        private class OtherAlgorithm : BaseAlgorithm
        {
            public override ResponseObject Start(AlgorithmSettings settings, ExchangeProvidersContainer container, DatabaseContext database)
            {
                throw new NotImplementedException();
            }
        }
        #pragma warning restore CA1812
    }
}