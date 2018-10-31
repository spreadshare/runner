using System;
using System.Collections.Generic;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.Models;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class TotalPortfolioTests : BaseTest
    {
        public TotalPortfolioTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void PortfolioIsBuild()
        {
            var portfolio = new TotalPortfolio();
            var allocation = new Assets(new List<ExchangeBalance>()
            {
                new ExchangeBalance(new Currency("ETH"), 1.0M, 0)
            });
            portfolio.SetAlgorithmAllocation(typeof(SimpleBandWagonAlgorithm), allocation);
            
            var amount = portfolio.GetAlgorithmAllocation(typeof(SimpleBandWagonAlgorithm))
                .GetAllocation(new Currency("ETH"));
            Assert.Equal(amount, 1.0M);
        }

        [Fact]
        public void TradeIsDigested()
        {
            Type alg = typeof(SimpleBandWagonAlgorithm);
            Currency from = new Currency("BTC");
            Currency to = new Currency("VET");
            
            var portfolio = new TotalPortfolio();
            var allocation = new Assets(new List<ExchangeBalance>()
            {
                new ExchangeBalance(from, 666.0M, 0.0M)
            });
            portfolio.SetAlgorithmAllocation(alg, allocation);
            
            var exec = new TradeExecution(
                new AssetValue(from, 50.0M),
                new AssetValue(to, 420420.0M), 
                typeof(SimpleBandWagonAlgorithm));
            
            portfolio.ApplyTradeExecution(exec);
            Assert.Equal(portfolio.GetAlgorithmAllocation(alg).GetAllocation(from), 616.0M);
            Assert.Equal(portfolio.GetAlgorithmAllocation(alg).GetAllocation(to), 420420.0M);
        }
    }
}