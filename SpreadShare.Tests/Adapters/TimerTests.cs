using System;
using SpreadShare.Strategy;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Adapters
{
    /// <summary>
    /// Collection of tests for the TimerAdapter
    /// </summary>
    public class TimerTests : BaseTest
    {
        private const int AllowedError = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        public TimerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests if the timer executes the callback right away if the provided time is 0.
        /// </summary>
        [Fact]
        public void TimerExecutesCallbackNoDelay()
        {
            bool flag = false;
            var timer = new Timer(0, () => flag = true);
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            System.Threading.Thread.Sleep(AllowedError);
            Assert.True(flag, $"Callback was not called right away (+/- {AllowedError}ms)");
        }

        /// <summary>
        /// Tests if the executes the callback after exactly one second.
        /// </summary>
        [Fact]
        public void TimerExecutesAfterOneSecond()
        {
            bool flag = false;
            var timer = new Timer(1000, () => flag = true);
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (GetPassedTime(start) < 1000 - AllowedError)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (flag)
            {
                Assert.True(false, $"Callback was called before one second (+/- {AllowedError}ms)");
            }

            while (GetPassedTime(start) < 1000 + AllowedError)
            {
                System.Threading.Thread.Sleep(1);
            }

            Assert.True(flag, $"Callback was not called after one second (+/- {AllowedError}ms)");
        }

        /// <summary>
        /// Tests if the timer executes after a certain amount of milliseconds not divisible by 1000
        /// </summary>
        [Fact]
        public void TimerExecutesWithPartialSeconds()
        {
            bool flag = false;
            var timer = new Timer(1337, () => flag = true);
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (GetPassedTime(start) < 1337 - AllowedError)
            {
                System.Threading.Thread.Sleep(1);
            }

            if (flag)
            {
                Assert.True(false, $"Callback was called before 1500 (+/-{AllowedError}) milliseconds");
            }

            while (GetPassedTime(start) < 1337 + AllowedError)
            {
                System.Threading.Thread.Sleep(1);
            }

            Assert.True(flag, $"Callback was not called after 1500 (+/- {AllowedError}) milliseconds");
        }

        /// <summary>
        /// Helper function that calculates the passed number of milliseconds
        /// </summary>
        /// <param name="start">UnixTime from (in ms)</param>
        /// <returns>Number of milliseconds since start</returns>
        private static long GetPassedTime(long start)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start;
        }
    }
}