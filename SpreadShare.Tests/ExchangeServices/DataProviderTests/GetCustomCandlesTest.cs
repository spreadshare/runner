using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class GetCustomCandlesTest : DataProviderTestUtils
    {
        public GetCustomCandlesTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void GetCustomCandlesNull()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            Assert.Throws<ArgumentNullException>(
                () => data.GetCandles(null, CandleWidth.FiveMinutes, 1));
        }

        [Fact]
        public void GetCustomCandlesSmaller()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.OneMinute, 1));
        }

        [Fact]
        public void GetCustomCandlesNotDivisble()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.DONOTUSETestEntry, 1));
        }

        [Fact]
        public void GetCustomCandlesCorrectAmountAllIdentity()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveMinutes, 12);
            Assert.Equal(12, candles.Length);
        }

        [Fact]
        public void GetCustomCandlesCorrectAmountPartialIdentity()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveMinutes, 5);
            Assert.Equal(5, candles.Length);
        }

        [Fact]
        public void GetCustomCandlesCorrectAmountAll()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveteenMinutes, 4);
            Assert.Equal(4, candles.Length);
        }

        [Fact]
        public void GetCustomCandlesCorrectAmountPartial()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveteenMinutes, 3);
            Assert.Equal(3, candles.Length);
        }

        [Fact]
        public void GetCustomCandlesNoPivotThirtyMinutes()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.ThirtyMinutes, 2);
            Assert.Equal(5.7M, candles[0].Close);
            Assert.Equal(5.6M, candles[1].Close);
        }

        [Fact]
        public void GetCustomCandlesNoPivotFiveteenMinutes()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveteenMinutes, 4);
            Assert.Equal(5.7M, candles[0].Close);
            Assert.Equal(6.9M, candles[1].Close);
            Assert.Equal(5.6M, candles[2].Close);
            Assert.Equal(8.01M, candles[3].Close);
        }

        [Fact]
        public void GetCustomCandlesOne()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderHappyFlowImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveteenMinutes, 1);
            Assert.Equal(6.2M, candles[0].Close);
        }

        [Fact]
        public void GetCustomCandlesMultiple()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderHappyFlowImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveteenMinutes, 3);
            Assert.Equal(6.2M, candles[0].Close);
            Assert.Equal(6.3M, candles[1].Close);
            Assert.Equal(8.872M, candles[2].Close);
        }

        [Fact]
        public void GetCustomCandlesIdentity()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderHappyFlowImplementation>();
            var candles = data.GetCandles(TradingPair.Parse("EOSETH"), CandleWidth.FiveMinutes, 1);
            Assert.Equal(5.7M, candles[0].Close);
        }

        // Class is instantiated via activator
        #pragma warning disable CA1812

        private class TimerProviderHappyFlowImplementation : TimerProviderTestImplementation
        {
            public TimerProviderHappyFlowImplementation(ILoggerFactory loggerFactory)
                : base(loggerFactory)
            {
            }

            public override DateTimeOffset CurrentTime => DateTimeOffset.FromUnixTimeMilliseconds(3600000L);

            public override DateTimeOffset Pivot => DateTimeOffset.FromUnixTimeMilliseconds(300000);

            public override void RunPeriodicTimer() => Expression.Empty();
        }

        private class TimerProviderNoPivotImplementation : TimerProviderTestImplementation
        {
            public TimerProviderNoPivotImplementation(ILoggerFactory loggerFactory)
                : base(loggerFactory)
            {
            }

            public override DateTimeOffset CurrentTime => DateTimeOffset.FromUnixTimeMilliseconds(3600000L);

            public override DateTimeOffset Pivot => DateTimeOffset.FromUnixTimeMilliseconds(0);

            public override void RunPeriodicTimer() => Expression.Empty();
        }

        private class DataProviderImplementation : DataProviderTestUtils.DataProviderTestImplementation
        {
            public DataProviderImplementation(ILoggerFactory loggerFactory, TimerProvider timerProvider)
                : base(loggerFactory, timerProvider)
            {
            }

            protected override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit)
            {
                // This array is reversed before it is returned.
                return new ResponseObject<BacktestingCandle[]>(
                    new[]
                    {
                        // #0
                        new BacktestingCandle(
                            timestamp: 0L,
                            open: 5,
                            close: 6.6M,
                            high: 7.2M,
                            low: 4.5M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #1
                        new BacktestingCandle(
                            timestamp: 300000L,
                            open: 5,
                            close: 6.6M,
                            high: 7.2M,
                            low: 4.5M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #2
                        new BacktestingCandle(
                            timestamp: 600000L,
                            open: 6.12M,
                            close: 8.01M,
                            high: 8.02M,
                            low: 6.0M,
                            volume: 3424,
                            tradingPair: "EOSETH"),

                        // #3
                        new BacktestingCandle(
                            timestamp: 900000L,
                            open: 7.90M,
                            close: 8.872M,
                            high: 8.9M,
                            low: 7.90M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #4
                        new BacktestingCandle(
                            timestamp: 1200000L,
                            open: 7.6M,
                            close: 6.8M,
                            high: 7.8M,
                            low: 6.8M,
                            volume: 20453,
                            tradingPair: "EOSETH"),

                        // #5
                        new BacktestingCandle(
                            timestamp: 1500000L,
                            open: 7.9M,
                            close: 5.6M,
                            high: 7.9M,
                            low: 5.6M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #6
                        new BacktestingCandle(
                            timestamp: 1800000L,
                            open: 5.9M,
                            close: 6.3M,
                            high: 6.6M,
                            low: 5.3M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #7
                        new BacktestingCandle(
                            timestamp: 2100000L,
                            open: 6.4M,
                            close: 6.6M,
                            high: 7.2M,
                            low: 6.4M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #8
                        new BacktestingCandle(
                            timestamp: 2400000L,
                            open: 6.5M,
                            close: 6.9M,
                            high: 7.4M,
                            low: 6.5M,
                            volume: 24053,
                            tradingPair: "EOSETH"),

                        // #9
                        new BacktestingCandle(
                            timestamp: 2700000L,
                            open: 6.7M,
                            close: 6.2M,
                            high: 6.8M,
                            low: 5.9M,
                            volume: 68453,
                            tradingPair: "EOSETH"),

                        // #10
                        new BacktestingCandle(
                            timestamp: 3000000L,
                            open: 6.2M,
                            close: 5.6M,
                            high: 6.4M,
                            low: 5.5M,
                            volume: 4053,
                            tradingPair: "EOSETH"),

                        // #11
                        new BacktestingCandle(
                            timestamp: 3300000L,
                            open: 5.6M,
                            close: 5.7M,
                            high: 5.8M,
                            low: 5.5M,
                            volume: 24053,
                            tradingPair: "EOSETH"),
                    }.Reverse().Take(limit).ToArray());
            }
        }

        #pragma warning disable
    }
}