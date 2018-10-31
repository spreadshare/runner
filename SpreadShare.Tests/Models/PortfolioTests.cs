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
            Currency c = new Currency("ETH");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, 1.0M, 0.0M )}
            });
            
            Assert.Equal(portfolio.GetAllocation(new Currency("ETH")).Free, 1.0M);
        }

        [Fact]
        public void BalancesAreSummed()
        {
            Currency c = new Currency("ETH");
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, 1.0M, 0.0M )}
            });
            
            var secondary = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, 3.0M, 5.0M) }
            });

            var result = Portfolio.Add(portfolio, secondary);
            Assert.Equal(result.GetAllocation(c).Free, 4.0M);
            Assert.Equal(portfolio.GetAllocation(c).Free, 1.0M);
            Assert.Equal(secondary.GetAllocation(c).Free, 3.0M);
        }
    }
}