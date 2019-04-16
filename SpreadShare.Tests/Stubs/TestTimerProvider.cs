using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.Tests.Stubs
{
    internal class TestTimerProvider : TimerProvider
    {
        public TestTimerProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override DateTimeOffset CurrentTime { get; }

        public override DateTimeOffset LastCandleOpen { get; }

        public override DateTimeOffset Pivot => DateTimeOffset.FromUnixTimeMilliseconds(0);

        public override void RunPeriodicTimer() => throw new NotImplementedException();

        public override void WaitForNextCandle() => throw new NotImplementedException();
    }
}