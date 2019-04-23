using System;
using Dawn;
using SpreadShare.Utilities;
using Xunit;

namespace SpreadShare.Tests.UtilitiesTests
{
    public class GuardExtensionsTests
    {
        [Fact]
        public void InRangeInclusiveHappyFlowDecimal()
        {
            var arg = 5M;
            Guard.Argument(arg).InRangeInclusive(4.9M, 5.1M);
        }

        [Fact]
        public void InRangeInclusiveOnTheEdgeDecimal()
        {
            var arg = 5M;
            Guard.Argument(arg).InRangeInclusive(5M, 5.1M);
        }

        [Fact]
        public void InRangeInclusiveOutsideDecimal()
        {
            var arg = 4M;
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Argument(arg).InRangeInclusive(4.5M, 5.1M));
        }

        [Fact]
        public void InRangeInclusiveOnTheEdgeGeneric()
        {
            var arg = new CompareTypeEqual();
            Guard.Argument(arg).InRangeInclusive(5M, 5.1M);
        }

        [Fact]
        public void InRangeInclusiveOutsideGeneric()
        {
            var arg = new CompareTypeGreater();
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Argument(arg).InRangeInclusive(4.5M, 5.1M));
        }

        [Fact]
        public void InRangeInclusiveNull()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Argument((IComparable)null).InRangeInclusive(0, 0));
        }

        private class CompareTypeGreater : IComparable
        {
            public int CompareTo(object obj) => 1;
        }

        private class CompareTypeEqual : IComparable
        {
            public int CompareTo(object obj) => 0;
        }
    }
}