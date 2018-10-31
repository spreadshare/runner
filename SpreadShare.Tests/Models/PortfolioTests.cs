using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class PortfolioTests : BaseTest
    {
        public PortfolioTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void PortfolioConstructor()
        {
            List<Balance> balances = new List<Balance>()
            {
                new Balance(new Currency("ETH"), 1.0M, 0.0M)
            };

            var dict = balances.ToDictionary(x => x.Symbol, x => x);
            var portfolio = new Portfolio(dict);
            
            Assert.Equal(portfolio.GetAllocation(new Currency("ETH")).Free, 1.0M);
        }
    }
}