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
    public class AverageTrueRangeTests : BaseProviderTests
    {
        private readonly DataProvider _data;

        public AverageTrueRangeTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var comms = ServiceProviderSingleton.Instance.ServiceProvider.GetService<BinanceCommunicationsService>();
            comms.Connect();
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            _data = container.DataProvider;
            var property = _data.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(SpreadShare.ExchangeServices.Providers.DataProvider)}");

            // Inject test implementation
            property.SetValue(_data, new DataProviderElevenCandlesImplementation(LoggerFactory, comms));
        }

        [Fact]
        public void AverageTrueRangeHappyFlow()
        {
            var atr = _data.GetAverageTrueRange(TradingPair.Parse("EOSETH"), 10, 2);
            Assert.Equal(2.75M, atr);
        }

        [Fact]
        public void AverageTrueRangeZeroOrNegative()
        {
            var pair = TradingPair.Parse("EOSETH");
            Assert.Throws<ArgumentOutOfRangeException>(() => _data.GetAverageTrueRange(pair, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _data.GetAverageTrueRange(pair, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _data.GetAverageTrueRange(pair, 5, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _data.GetAverageTrueRange(pair, 5, -1));
        }

        [Fact]
        public void AverageTrueRangeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _data.GetAverageTrueRange(null, 5));
        }

        [Fact]
        public void AverageTrueRangeNotMultiple()
        {
            var pair = TradingPair.Parse("EOSETH");
            Assert.Throws<ArgumentException>(() => _data.GetAverageTrueRange(pair, 10, 6));
            Assert.Throws<ArgumentException>(() => _data.GetAverageTrueRange(pair, 10, 20));
        }

        internal class DataProviderElevenCandlesImplementation : AbstractDataProvider
        {
            public DataProviderElevenCandlesImplementation(ILoggerFactory loggerFactory, ExchangeCommunications exchangeCommunications)
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

                        // #11
                        new BacktestingCandle(
                            timestamp: 33000000L,
                            open: 5.6M,
                            close: 5.7M,
                            high: 5.8M,
                            low: 5.5M,
                            volume: 24053,
                            tradingPair: "EOSETH"),
                    }.Reverse().ToArray());
            }
        }
    }
}