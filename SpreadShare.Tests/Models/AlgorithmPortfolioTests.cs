using System.Collections.Generic;
using System.Threading.Tasks;
using SpreadShare.Models;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class AlgorithmPortfolioTests : BaseTest
    {
        public AlgorithmPortfolioTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void AllocationIsAllocated()
        {
            List<ExchangeBalance> balances = new List<ExchangeBalance>() 
                { new ExchangeBalance(new Currency("ETH"), 1.0M, 0.0M) };
            var assets = new Assets(balances);
            var allocation = new AlgorithmPortfolio(assets);
            Assert.Equal(allocation.GetAllocation(new Currency("ETH")), 1.0M);
            Assert.Equal(allocation.GetAllocation(new Currency("NONCOIN")), 0.0M);
        }

        [Fact]
        public void AllocationsAreSummed()
        {
            List<ExchangeBalance> firstList = new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 1.0M, 0.0M)
            };
            
            List<ExchangeBalance> secondList = new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 1.5M, 0.0M)
            };
            
            var first = new Assets(firstList);
            var second = new Assets(secondList);
            var summed = first.Combine(second);
            Assert.Equal(summed.GetFreeBalance(new Currency("ETH")), 2.5M);
            Assert.Equal(summed.GetFreeBalance(new Currency("NONCOIN")), 0.0M);
        }

        [Fact]
        public void NonOverlappingAllocationsAreSummed()
        {
            List<ExchangeBalance> firstList = new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 1.0M, 0.0M),
                new ExchangeBalance(new Currency("VET"), 42.0M, 0.0M) 
            };
            
            List<ExchangeBalance> secondList = new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 1.5M, 0.0M),
                new ExchangeBalance(new Currency("BTC"), 69.0M, 0.0M)
            };
            
            var first = new Assets(firstList);
            var second = new Assets(secondList);
            var summed = first.Combine(second);
            Assert.Equal(summed.GetFreeBalance(new Currency("ETH")), 2.5M);
            Assert.Equal(summed.GetFreeBalance(new Currency("VET")), 42.0M);
            Assert.Equal(summed.GetFreeBalance(new Currency("BTC")), 69.0M);
            Assert.Equal(summed.GetFreeBalance(new Currency("NONCOIN")), 0.0M);
        }

        [Fact]
        public void TradeExecutionIsDigested()
        {
            var allocation = new AlgorithmPortfolio(new Assets(new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 2.9M, 0.0M)
            }));
            var from = new AssetValue(new Currency("ETH"), 2.0M);
            var to = new AssetValue(new Currency("BNB"), 4000.0M);
            var exec = new TradeExecution(from, to, typeof(bool));
            allocation.UpdateAllocation(exec);
            Assert.Equal(allocation.GetAllocation(new Currency("ETH")), 0.9M);
        }
    }
}