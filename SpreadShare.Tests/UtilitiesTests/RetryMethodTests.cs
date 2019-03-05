using System;
using System.Collections.Generic;
using SpreadShare.Models;
using SpreadShare.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.UtilitiesTests
{
    public class RetryMethodTests : BaseTest
    {
        private static int _retryMethodFiveRetriesCounter;
        private static int _retryMethodTenRetriesCounter;
        private static int _retryMethodBackoffCounter;
        private static DateTimeOffset _retryMethodBackoffPreviousStamp;

        public RetryMethodTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void RetryMethodFirstTimeSuccess()
        {
            var query = HelperMethods.RetryMethod(RetryMethodFirstTimeSuccessImplementation, Logger, 5, 0);
            Assert.Equal(ResponseCode.Success, query.Code);
        }

        [Fact]
        public void RetryMethodMethodNull()
        {
            Assert.Throws<ArgumentNullException>(() => HelperMethods.RetryMethod(null, Logger));
        }

        [Fact]
        public void RetryMethodLoggerCanBeNull()
        {
            var query = HelperMethods.RetryMethod(RetryMethodFirstTimeSuccessImplementation, null);
            Assert.Equal(ResponseCode.Success, query.Code);
        }

        [Fact]
        public void RetryMethodMaxRetriesZeroOrNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HelperMethods.RetryMethod(RetryMethodFirstTimeSuccessImplementation, Logger, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HelperMethods.RetryMethod(RetryMethodFirstTimeSuccessImplementation, Logger, -1));
        }

        [Fact]
        public void RetryMethodBackoffMillisNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HelperMethods.RetryMethod(RetryMethodFirstTimeSuccessImplementation, Logger, 5, -1));
        }

        [Fact]
        public void RetryMethodFiveRetries()
        {
            var query = HelperMethods.RetryMethod(RetryMethodFiveRetriesImplementation, Logger, 5, 0);
            Assert.Equal(ResponseCode.Success, query.Code);
        }

        [Fact]
        public void RetryMethodTenRetries()
        {
            var queryDefault = HelperMethods.RetryMethod(RetryMethodTenRetriesImplementation, Logger, 5, 0);
            var queryOverridden = HelperMethods.RetryMethod(RetryMethodTenRetriesImplementation, Logger, 10, 0);
            Assert.Equal(ResponseCode.Error, queryDefault.Code);
            Assert.Equal(ResponseCode.Success, queryOverridden.Code);
        }

        [Theory]
        [InlineData(5, 100)]
        [InlineData(3, 150)]
        [InlineData(2, 400)]
        public void RetryMethodBackoff(int maxRetries, int backoffMillis)
        {
            _retryMethodBackoffPreviousStamp = DateTimeOffset.Now;
            _retryMethodBackoffCounter = 0;
            HelperMethods.RetryMethod(() => RetryMethodBackOffImplementation(backoffMillis), Logger, maxRetries, backoffMillis);
        }

        private static ResponseObject RetryMethodBackOffImplementation(int backoffMillis)
        {
            // Calculate the time difference between the previous call and now.
            var diff = DateTimeOffset.Now - _retryMethodBackoffPreviousStamp;
            _retryMethodBackoffPreviousStamp = DateTimeOffset.Now;

            // Calculate the target time difference (backoffMillis * 2 ^ count)
            var targetSpan = TimeSpan.FromMilliseconds(backoffMillis);
            if (_retryMethodBackoffCounter == 0)
            {
                targetSpan = TimeSpan.Zero;
            }
            else
            {
                for (int i = 0; i < _retryMethodBackoffCounter; i++)
                {
                    targetSpan *= 2;
                }
            }

            Assert.Equal(diff, targetSpan, new TimeSpanComparerTenMilliseconds());

            _retryMethodBackoffCounter++;
            return new ResponseObject(ResponseCode.Error);
        }

        private ResponseObject RetryMethodFirstTimeSuccessImplementation()
        {
            return new ResponseObject(ResponseCode.Success);
        }

        private ResponseObject RetryMethodFiveRetriesImplementation()
        {
            if (_retryMethodFiveRetriesCounter++ == 4)
            {
                return new ResponseObject(ResponseCode.Success);
            }

            return new ResponseObject(ResponseCode.Error);
        }

        private ResponseObject RetryMethodTenRetriesImplementation()
        {
            if (_retryMethodTenRetriesCounter++ == 9)
            {
                return new ResponseObject(ResponseCode.Success);
            }

            return new ResponseObject(ResponseCode.Error);
        }

        private class TimeSpanComparerTenMilliseconds : IEqualityComparer<TimeSpan>
        {
            public bool Equals(TimeSpan x, TimeSpan y)
            {
                var diff = Math.Abs((x - y).TotalMilliseconds);
                if (diff < 10)
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(TimeSpan obj) => obj.GetHashCode();
        }
    }
}