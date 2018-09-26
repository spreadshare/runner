using Xunit;
using SpreadShare.Models;

namespace UnitTests
{   
    public class ShouldSucceed
    {
        private Assets _assets;

        public ShouldSucceed() {
            _assets = new Assets();
        }

        [Fact]
        public void MoneyIsZero() {
            var result = _assets.GetTotalBalance(new Currency("MoonCoin"));
            Assert.True(result==0, "Non existing currencies should return a quantity of 0");
        }

        [Fact]
        public void Lala() {
            Assert.True(true, "wat even");
        }
    }
}
