using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class RateOfChangeTests : CandleTest
    {
        [Fact]
        public void RateOfChangeAll()
        {
            var roc = Candles.RateOfChange();
            Assert.Equal(-0.1363636363636363636363636364M, roc);
        }

        [Fact]
        public void RateOfChangeEmptySet()
        {
            var data = Array.Empty<BacktestingCandle>();
            Assert.Throws<InvalidOperationException>(() => data.RateOfChange());
        }

        [Fact]
        public void RateOfChangeNull()
        {
            BacktestingCandle[] data = null;
            Assert.Throws<ArgumentNullException>(() => data.RateOfChange());
        }

        [Fact]
        public void RateOfChangeOneCandle()
        {
            var roc = Candles.Take(1).RateOfChange();
            Assert.Equal(0, roc);
        }

        [Fact]
        public void RateOfChangeNegative()
        {
            var data = Candles.Skip(2).Take(3);
            var roc = data.RateOfChange();
            Assert.Equal(-0.151061173533083645443196005M, roc);
        }
    }
}