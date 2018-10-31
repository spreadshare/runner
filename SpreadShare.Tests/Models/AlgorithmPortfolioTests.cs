using System.Collections.Generic;
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
                { new ExchangeBalance(new Currency("ETH"), 1, 0) };
            var assets = new Assets(balances);
            var allocation = new AlgorithmPortfolio(assets);
            Assert.Equal(allocation.GetAllocation(new Currency("ETH")), 1.0M);
        }
    }
}