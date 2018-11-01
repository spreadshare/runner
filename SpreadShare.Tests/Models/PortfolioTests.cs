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

        [Theory]
        [InlineData("ETH", 1.0, 0.0, 3.0, 5.0)]
        public void BalancesAreSummed(string currency, decimal free1, decimal locked1, decimal free2, decimal locked2)
        {
            Currency c = new Currency(currency);
            var portfolio = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, free1, locked1 )}
            });
            
            var secondary = new Portfolio(new Dictionary<Currency, Balance>()
            {
                { c, new Balance(c, free2, locked2) }
            });

            var result = Portfolio.Add(portfolio, secondary);
            Assert.Equal(result.GetAllocation(c).Free, free1 + free2);
            Assert.Equal(portfolio.GetAllocation(c).Free, free1);
            Assert.Equal(secondary.GetAllocation(c).Free, free2);
            Assert.Equal(result.GetAllocation(c).Locked, locked1 + locked2);
        }
    }
}