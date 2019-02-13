using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.ExchangeCommunicationService;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public class StandardMovingAverageTests : BaseProviderTests
    {
        private DataProvider _data;

        public StandardMovingAverageTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var comms = ServiceProviderSingleton.Instance.ServiceProvider.GetService<BinanceCommunicationsService>();
            comms.Connect();
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            _data = container.DataProvider;
            var property = _data.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(SpreadShare.ExchangeServices.Providers.DataProvider)}");

            // Inject test implementation
            property.SetValue(_data, new DataProviderTenCandlesImplementation(LoggerFactory, comms));
        }

        [Theory]
        [InlineData(0, 5, 2, 0)]
        [InlineData(1, 2, 5, 0)]
        [InlineData(2, 1, 5, 5)]
        [InlineData(3, 2, 3, 3)]
        public void GetStandardMovingAverageHappyFlow(int id, int candlesPerInterval, int intervals, int offset)
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), candlesPerInterval, intervals, offset);
            var answers = new Dictionary<int, decimal>
            {
                { 0, 5.6M },
                { 1, 6.722M },
                { 2, 7.1764M },
                { 3, 7.024M },
            };

            Assert.Equal(answers[id], sma);
        }

        [Fact]
        public void GetStandardMovingAverageSingularSegment()
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), 10, 1);
            Assert.Equal(5.6M, sma);
        }

        [Fact]
        public void GetStandardMovingAverageSingularSegmentOffset()
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), 9, 1, 1);
            Assert.Equal(6.2M, sma);
        }

        [Fact]
        public void GetStandardMovingAverageSingularCandleSegment()
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), 1, 1);
            Assert.Equal(5.6M, sma);
        }

        [Fact]
        public void GetStandardMovingAverageSingularCandleSegmentOffset()
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), 1, 1, 2);
            Assert.Equal(6.9M, sma);
        }

        [Fact]
        public void GetStandardMovingAverageOneCandleSegments()
        {
            var sma = _data.GetStandardMovingAverage(TradingPair.Parse("EOSETH"), 1, 10);
            Assert.Equal(6.7482M, sma);
        }

        [Fact]
        public void GetStandardMovingAverageNull()
        {
            Assert.Throws<ArgumentNullException>(() => _data.GetStandardMovingAverage(null, 2, 10));
        }

        [Theory]
        [InlineData(2, 0, 0)]
        [InlineData(2, -1, 0)]
        [InlineData(0, 2, 0)]
        [InlineData(-1, 2, 0)]
        [InlineData(1, 10, -1)]
        public void GetStandardMovingAverageZeroOrNegative(int candlesPerInterval, int intervals, int offset)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _data.GetStandardMovingAverage(
                TradingPair.Parse("EOSETH"),
                candlesPerInterval,
                intervals,
                offset));
        }

        private class DataProviderTenCandlesImplementation : AbstractDataProvider
        {
            public DataProviderTenCandlesImplementation(ILoggerFactory loggerFactory, ExchangeCommunications exchangeCommunications)
                : base(loggerFactory, exchangeCommunications)
            {
            }

            public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack) => throw new NotImplementedException();

            public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack) => throw new NotImplementedException();

            public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width)
            {
                // This array is reversed before it is returned.
                return new ResponseObject<BacktestingCandle[]>(
                    new[]
                    {
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
                            timestamp: 30000000L,
                            open: 6.2M,
                            close: 5.6M,
                            high: 6.4M,
                            low: 5.5M,
                            volume: 4053,
                            tradingPair: "EOSETH"),
                    }.Reverse().Take(limit).ToArray());
            }
        }
    }
}