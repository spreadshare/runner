using Xunit;
using SpreadShare.Strategy;
using Xunit.Abstractions;
using System;
using SpreadShare.Tests;

namespace Tests.AdapterTests
{
    public class TimerTests : BaseTest
    {
        public TimerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void TimerExecutesCallbackNoDelay()
        {
            bool flag = false;
            var timer = new Timer(0, () => flag = true);
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            //Execute within 10 ms (be generous for the scheduler)
            System.Threading.Thread.Sleep(5);
            Assert.True(flag, "Callback was not called right away (+/- 5ms)");
        }

        [Fact]
        public void TimerExecutesAfterOneSecond()
        {
            bool flag = false;
            var timer = new Timer(1000, () => flag = true);
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            while(GetPassedTime(start) < 995) { System.Threading.Thread.Sleep(1); }
            if (flag) Assert.True(false, "Callback was called before one second (+/- 5ms)");

            while(GetPassedTime(start) < 1005) { System.Threading.Thread.Sleep(1); }
            Assert.True(flag, "Callback was not called after one second (+/- 5ms)");
        }

        [Fact]
        public void TimerExecutesWithPartialSeconds()
        {
            bool flag = false;
            var timer = new Timer(1337, () => flag = true);
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            while(GetPassedTime(start) < 1337-5) { System.Threading.Thread.Sleep(1); }
            if (flag) Assert.True(false, "Callback was called before 1500 (+/- 5) milliseconds");
            
            while(GetPassedTime(start) < 1337+5) { System.Threading.Thread.Sleep(1); }
            Assert.True(flag, "Callback was not called after 1500 (+/- 5) milliseconds");
        }

        private long GetPassedTime(long start)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
        }
    }
}