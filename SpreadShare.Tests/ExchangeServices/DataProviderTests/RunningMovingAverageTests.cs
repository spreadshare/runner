using System;
using System.Collections.Generic;
using SpreadShare.ExchangeServices.Providers;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class RunningMovingAverageTests
    {
        [Fact]
        public void RunningMovingAverageNull()
        {
            IEnumerable<decimal> input = null;
            Assert.Throws<ArgumentNullException>(() => input.RunningMovingAverage());
        }

        [Fact]
        public void RunningMovingAverageEmpty()
        {
            IEnumerable<decimal> input = Array.Empty<decimal>();
            Assert.Throws<InvalidOperationException>(() => input.RunningMovingAverage());
        }

        [Fact]
        public void RunningMovingAverageSingle()
        {
            IEnumerable<decimal> input = new[] { 5M };
            Assert.Equal(5, input.RunningMovingAverage());
        }

        [Fact]
        public void RunningMovingAverageHappyFlow()
        {
            IEnumerable<decimal> input = new[] { 5M, 2M, 1M, 0M, 7M, 5M, 4M };
            Assert.Equal(4.1105151764995877568020127667M, input.RunningMovingAverage());
        }
    }
}