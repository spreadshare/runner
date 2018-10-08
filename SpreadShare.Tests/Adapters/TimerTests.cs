using System;
using SpreadShare.Strategy;
using SpreadShare.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Tests.AdapterTests
{
    /// <summary>
    /// Collection of tests for the TimerAdapter
    /// </summary>
    public class TimerTests : BaseTest
    {
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
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Execute within 10 ms (be generous for the scheduler)
            System.Threading.Thread.Sleep(5);
            Assert.True(flag, "Callback was not called right away (+/- 5ms)");
        }

        /// <summary>
        /// Tests if the executes the callback after exactly one second.
        /// </summary>
        [Fact]
        public void TimerExecutesAfterOneSecond()
        {
            bool flag = false;
            var timer = new Timer(1000, () => flag = true);
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            while (GetPassedTime(start) < 995) { System.Threading.Thread.Sleep(1); }
            if (flag) { Assert.True(false, "Callback was called before one second (+/- 5ms)"); }

            while (GetPassedTime(start) < 1005) { System.Threading.Thread.Sleep(1); }
            Assert.True(flag, "Callback was not called after one second (+/- 5ms)");
        }

        /// <summary>
        /// Tests if the timer executes after a certain amount of milliseconds not divisible by 1000
        /// </summary>
        [Fact]
        public void TimerExecutesWithPartialSeconds()
        {
            bool flag = false;
            var timer = new Timer(1337, () => flag = true);
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            while (GetPassedTime(start) < 1337 - 5) { System.Threading.Thread.Sleep(1); }
            if (flag) { Assert.True(false, "Callback was called before 1500 (+/- 5) milliseconds"); }

            while (GetPassedTime(start) < 1337 + 5) { System.Threading.Thread.Sleep(1); }
            Assert.True(flag, "Callback was not called after 1500 (+/- 5) milliseconds");
        }

        /// <summary>
        /// Helper function that calculates the passed number of milliseconds
        /// </summary>
        /// <param name="start">UnixTime from (in ms)</param>
        /// <returns>Number of milliseconds since start</returns>
        private static long GetPassedTime(long start)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
        }
    }
}