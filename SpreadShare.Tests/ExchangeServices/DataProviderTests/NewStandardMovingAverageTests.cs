using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class NewStandardMovingAverageTests : DataProviderTestUtils
    {
        public NewStandardMovingAverageTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void StandardMovingAverageSingleCandle()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCustomCandles(TradingPair.Parse("EOSETH"), 1, CandleWidth.FiveMinutes);
            var sma = candles.StandardMovingAverage();
            Assert.Equal(5.7M, sma);
        }

        [Fact]
        public void StandardMovingAverageFourCandles()
        {
            var data = GetDataProviderWithTimer<DataProviderImplementation, TimerProviderNoPivotImplementation>();
            var candles = data.GetCustomCandles(TradingPair.Parse("EOSETH"), 4, CandleWidth.FiveteenMinutes);
            var sma = candles.StandardMovingAverage();
            Assert.Equal(6.5525M, sma);
        }

        [Fact]
        public void StandardMovingAverageNull()
        {
            BacktestingCandle[] lol = null;
            Assert.Throws<ArgumentNullException>(() => lol.StandardMovingAverage());
        }

        [Fact]
        public void StandardMovingAverageEmptySet()
        {
            var candles = Array.Empty<BacktestingCandle>();
            Assert.Throws<InvalidOperationException>(
                () => candles.StandardMovingAverage());
        }

        // Class is instantiated via activator
        #pragma warning disable CA1812

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
            public DataProviderImplementation(ILoggerFactory loggerFactory, ExchangeCommunications exchangeCommunications)
                : base(loggerFactory, exchangeCommunications)
            {
            }

            public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width)
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