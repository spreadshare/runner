using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class RelativeStrengthIndexTests : CandleTest
    {
        [Fact]
        public void RelativeStrengthIndexNull()
        {
            IEnumerable<BacktestingCandle> input = null;
            Assert.Throws<ArgumentNullException>(() => input.RelativeStrengthIndex());
        }

        [Fact]
        public void RelativeStrengthIndexEmpty()
        {
            var input = Array.Empty<BacktestingCandle>();
            Assert.Throws<InvalidOperationException>(() => input.RelativeStrengthIndex());
        }

        [Fact]
        public void RelativeStrengthIndexSingle()
        {
            var input = Candles.Take(1);
            Assert.Throws<InvalidOperationException>(() => input.RelativeStrengthIndex());
        }

        [Fact]
        public void RelativeStrengthIndexHappyFlow()
        {
            var input = Candles.Take(10);
            var rsi = input.RelativeStrengthIndex();
            Assert.Equal(1, rsi);
        }
    }
}