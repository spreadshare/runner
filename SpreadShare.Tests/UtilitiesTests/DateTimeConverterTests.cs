using System;
using SpreadShare.Utilities;
using Xunit;

namespace SpreadShare.Tests.UtilitiesTests
{
    public class DateTimeConverterTests
    {
        [Fact]
        public void DateTimeConverterZero()
        {
            var input = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var stamp = input.ToUnixTimestampMilliseconds();
            Assert.Equal(0, stamp);
        }

        [Fact]
        public void DateTimeConverterUtc()
        {
            var input = new DateTime(1994, 4, 5, 0, 0, 1, DateTimeKind.Utc);
            var stamp = input.ToUnixTimestampMilliseconds();
            var utcStamp = new DateTimeOffset(1994, 4, 5, 0, 0, 1, TimeSpan.Zero).ToUnixTimeMilliseconds();
            Assert.Equal(utcStamp, stamp);
        }

        [Fact]
        public void DateTimeConverterLocalNow()
        {
            var input = DateTime.Now;
            var stamp = input.ToUnixTimestampMilliseconds();
            var utcStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var diff = Math.Abs(stamp - utcStamp);
            Assert.True(diff < 500);
        }

        [Fact]
        public void DateTimeConverterUtcNow()
        {
            var input = DateTime.UtcNow;
            var stamp = input.ToUnixTimestampMilliseconds();
            var utcStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var diff = Math.Abs(stamp - utcStamp);
            Assert.True(diff < 500);
        }

        [Fact]
        public void DateTimeMin()
        {
            var input = DateTime.MinValue;
            var stamp = input.ToUnixTimestampMilliseconds();
            var referenceStamp = new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
            Assert.Equal(referenceStamp, stamp);
        }
    }
}