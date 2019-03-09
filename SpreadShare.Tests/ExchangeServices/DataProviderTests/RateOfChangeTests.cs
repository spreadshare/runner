using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Database;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class RateOfChangeTests
    {
        // This array is reversed before it is returned.
        private readonly BacktestingCandle[] _candles =
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
            };

        [Fact]
        public void RateOfChangeAll()
        {
            var roc = _candles.RateOfChange();
            Assert.Equal(0.1578947368421052631578947368M, roc);
        }

        [Fact]
        public void RateOfChangeEmptySet()
        {
            var data = Array.Empty<BacktestingCandle>();
            Assert.Throws<InvalidOperationException>(() => data.RateOfChange());
        }

        [Fact]
        public void RateOfChangeNull()
        {
            BacktestingCandle[] data = null;
            Assert.Throws<ArgumentNullException>(() => data.RateOfChange());
        }

        [Fact]
        public void RateOfChangeOneCandle()
        {
            var roc = _candles.Take(1).RateOfChange();
            Assert.Equal(0, roc);
        }

        [Fact]
        public void RateOfChangeNegative()
        {
            var data = _candles.Take(3);
            var roc = data.RateOfChange();
            Assert.Equal(-0.1760299625468164794007490637M, roc);
        }
    }
}