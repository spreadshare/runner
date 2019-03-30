using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.ExchangeServices.DataProviderTests;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BacktestProviderTests
{
    public class DataProviderBacktestBufferVsProvider : DataProviderTestUtils
    {
        public DataProviderBacktestBufferVsProvider(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void HighestHighIsMaximumOfCandleHighs()
        {
            const string source = @"
               TradingPairs: [EOSETH]
               CandleWidth: 5
            ";
            var config = ParseAlgorithmConfiguration(source);
            var data = GetDataProvider<DataProviderGetCandlesImplementation>(config);
            var pair = TradingPair.Parse("EOSETH");
            var candles = data.GetCandles(pair, 1300);
            var highestHigh = data.GetHighestHigh(pair, 1300);
            Assert.Equal(candles.Max(x => x.High), highestHigh);
        }

        [Fact]
        public void LowestLowIsMaximumOfCandleLows()
        {
            const string source = @"
               TradingPairs: [EOSETH]
               CandleWidth: 5
            ";
            var config = ParseAlgorithmConfiguration(source);
            var data = GetDataProvider<DataProviderGetCandlesImplementation>(config);
            var pair = TradingPair.Parse("EOSETH");
            var candles = data.GetCandles(pair, 1300);
            var lowestLow = data.GetLowestLow(pair, 1300);
            Assert.Equal(candles.Min(x => x.Low), lowestLow);
        }

        // Class is instantiated via the Activator
        #pragma warning disable CA1812

        private class DataProviderGetCandlesImplementation : DataProviderTestImplementation
        {
            public DataProviderGetCandlesImplementation(ILoggerFactory loggerFactory, TimerProvider timerProvider)
                : base(loggerFactory, timerProvider)
            {
            }

            public override ResponseObject<decimal> GetHighestHigh(TradingPair pair, int width, int numberOfCandles)
            {
                var candles = GetCandles(pair, numberOfCandles).Data;
                var method = typeof(BacktestBuffers)
                    .GetMethod("BuildHighestHighBuffer", BindingFlags.NonPublic | BindingFlags.Static);
                var highestHighBuffer = (decimal[])method.Invoke(null, new object[] { candles, numberOfCandles });
                return new ResponseObject<decimal>(highestHighBuffer.Last());
            }

            public override ResponseObject<decimal> GetLowestLow(TradingPair pair, int width, int numberOfCandles)
            {
                var candles = GetCandles(pair, numberOfCandles).Data;
                var method = typeof(BacktestBuffers)
                    .GetMethod("BuildLowestLowBuffer", BindingFlags.NonPublic | BindingFlags.Static);
                var lowestLowBuffer = (decimal[])method.Invoke(null, new object[] { candles, numberOfCandles });
                return new ResponseObject<decimal>(lowestLowBuffer.Last());
            }

            protected override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit)
            {
                var random = new Random("$pread$hare".GetHashCode(StringComparison.InvariantCulture));
                var result = new BacktestingCandle[limit];

                for (int i = 0; i < limit; i++)
                {
                    var open = (decimal)(random.NextDouble() * 30) + 1;
                    var close = (decimal)(random.NextDouble() * 30) + 1;
                    var high = (decimal)(random.NextDouble() * 30) + 1;
                    var low = (decimal)(random.NextDouble() * 30) + 1;
                    var volume = (decimal)(random.NextDouble() * 420);

                    result[i] = new BacktestingCandle(
                        timestamp: i * 60000,
                        open: open,
                        close: close,
                        high: high,
                        low: low,
                        volume: volume,
                        tradingPair: "EOSETH");
                }

                return new ResponseObject<BacktestingCandle[]>(result);
            }
        }

        #pragma warning restore CA1812
    }
}