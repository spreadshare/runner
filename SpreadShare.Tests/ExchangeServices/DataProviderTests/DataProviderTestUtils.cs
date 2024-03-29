using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Tests.Stubs;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public abstract class DataProviderTestUtils : BaseProviderTests
    {
        protected DataProviderTestUtils(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        internal AlgorithmConfiguration ParseAlgorithmConfiguration(string source)
        {
            var config = new DeserializerBuilder().Build()
                    .Deserialize<TemplateAlgorithmConfiguration>(new StringReader(source));
            ConfigurationValidator.ValidateConstraintsRecursively(AlgorithmConfiguration);
            return config;
        }

        internal DataProvider GetDataProvider<T>(AlgorithmConfiguration config)
            where T : DataProviderTestImplementation
        {
            var implementation = (T)Activator.CreateInstance(typeof(T), LoggerFactory, new TestTimerProvider(LoggerFactory));
            var data = new DataProvider(LoggerFactory, implementation, config);
            return data;
        }

        internal DataProvider GetDataProviderWithTimer<TD, TT>(AlgorithmConfiguration config)
            where TD : DataProviderTestImplementation
            where TT : TimerProviderTestImplementation
        {
            var implementation = (TD)Activator.CreateInstance(typeof(TD), LoggerFactory, new TestTimerProvider(LoggerFactory));
            var timerProperty = implementation.GetType()
                               .GetProperty("TimerProvider", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception(
                               $"Expected property 'TimerProvider' on {implementation.GetType().Name}");
            var timerProvider = Activator.CreateInstance(typeof(TT), LoggerFactory);
            timerProperty.SetValue(implementation, timerProvider);
            return new DataProvider(LoggerFactory, implementation, config);
        }

        internal abstract class TimerProviderTestImplementation : TimerProvider
        {
            protected TimerProviderTestImplementation(ILoggerFactory loggerFactory)
                : base(loggerFactory)
            {
            }

            public abstract override DateTimeOffset CurrentTime { get; }

            public abstract override DateTimeOffset Pivot { get; }

            public abstract override DateTimeOffset LastCandleOpen { get; }

            public abstract override void RunPeriodicTimer();
        }

        internal class DataProviderTestImplementation : AbstractDataProvider
        {
            public DataProviderTestImplementation(ILoggerFactory loggerFactory, TimerProvider timerProvider)
                : base(loggerFactory, timerProvider)
            {
            }

            public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetHighestHigh(TradingPair pair, int width, int numberOfCandles) => throw new NotImplementedException();

            public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack) => throw new NotImplementedException();

            protected override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit) => throw new NotImplementedException();
        }
    }
}