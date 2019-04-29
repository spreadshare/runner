using SpreadShare.Models.Database;

namespace SpreadShare.Tests.ExchangeServices
{
    public class CandleTest
    {
        protected readonly BacktestingCandle[] Candles =
            {
                // #0
                new BacktestingCandle(
                    openTimestamp: 0L,
                    open: 5,
                    close: 6.6M,
                    high: 7.2M,
                    low: 4.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #1
                new BacktestingCandle(
                    openTimestamp: 300000L,
                    open: 5,
                    close: 6.6M,
                    high: 7.2M,
                    low: 4.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #2
                new BacktestingCandle(
                    openTimestamp: 600000L,
                    open: 6.12M,
                    close: 8.01M,
                    high: 8.02M,
                    low: 6.0M,
                    volume: 3424,
                    tradingPair: "EOSETH"),

                // #3
                new BacktestingCandle(
                    openTimestamp: 900000L,
                    open: 7.90M,
                    close: 8.872M,
                    high: 8.9M,
                    low: 7.90M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #4
                new BacktestingCandle(
                    openTimestamp: 1200000L,
                    open: 7.6M,
                    close: 6.8M,
                    high: 7.8M,
                    low: 6.8M,
                    volume: 20453,
                    tradingPair: "EOSETH"),

                // #5
                new BacktestingCandle(
                    openTimestamp: 1500000L,
                    open: 7.9M,
                    close: 5.6M,
                    high: 7.9M,
                    low: 5.6M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #6
                new BacktestingCandle(
                    openTimestamp: 1800000L,
                    open: 5.9M,
                    close: 6.3M,
                    high: 6.6M,
                    low: 5.3M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #7
                new BacktestingCandle(
                    openTimestamp: 2100000L,
                    open: 6.4M,
                    close: 6.6M,
                    high: 7.2M,
                    low: 6.4M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #8
                new BacktestingCandle(
                    openTimestamp: 2400000L,
                    open: 6.5M,
                    close: 6.9M,
                    high: 7.4M,
                    low: 6.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),

                // #9
                new BacktestingCandle(
                    openTimestamp: 2700000L,
                    open: 6.7M,
                    close: 6.2M,
                    high: 6.8M,
                    low: 5.9M,
                    volume: 68453,
                    tradingPair: "EOSETH"),

                // #10
                new BacktestingCandle(
                    openTimestamp: 3000000L,
                    open: 6.2M,
                    close: 5.6M,
                    high: 6.4M,
                    low: 5.5M,
                    volume: 4053,
                    tradingPair: "EOSETH"),

                // #11
                new BacktestingCandle(
                    openTimestamp: 3300000L,
                    open: 5.6M,
                    close: 5.7M,
                    high: 5.8M,
                    low: 5.5M,
                    volume: 24053,
                    tradingPair: "EOSETH"),
            };
    }
}