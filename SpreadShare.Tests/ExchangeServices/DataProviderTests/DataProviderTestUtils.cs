using System;
using System.Collections.Generic;
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
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.DataProviderTests
{
    public abstract class DataProviderTestUtils : BaseProviderTests
    {
        protected DataProviderTestUtils(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        internal DataProvider GetDataProvider<T>()
            where T : DataProviderTestImplementation
        {
            var comms = ServiceProviderSingleton.Instance.ServiceProvider.GetService<BinanceCommunicationsService>();
            comms.Connect();
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            var data = container.DataProvider;
            var property = data.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(DataProvider)}");

            // Inject test implementation
            var implementation = Activator.CreateInstance(typeof(T), LoggerFactory, comms);
            property.SetValue(data, implementation);
            return data;
        }

        internal DataProvider GetDataProviderWithTimer<TD, TT>()
            where TD : DataProviderTestImplementation
            where TT : TimerProviderTestImplementation
        {
            var data = GetDataProvider<TD>();
            var property = data.GetType().GetProperty("TimerProvider", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'TimerProvider' on {nameof(DataProvider)}");

            // Inject test timer
            var timer = Activator.CreateInstance(typeof(TT), LoggerFactory);
            property.SetValue(data, timer);
            return data;
        }

        internal abstract class TimerProviderTestImplementation : TimerProvider
        {
            protected TimerProviderTestImplementation(ILoggerFactory loggerFactory)
                : base(loggerFactory)
            {
            }

            public abstract override DateTimeOffset CurrentTime { get; }

            public abstract override DateTimeOffset Pivot { get; }

            public abstract override void RunPeriodicTimer();
        }

        internal class DataProviderTestImplementation : AbstractDataProvider
        {
            public DataProviderTestImplementation(ILoggerFactory loggerFactory, ExchangeCommunications exchangeCommunications)
                : base(loggerFactory, exchangeCommunications)
            {
            }

            public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack) => throw new NotImplementedException();

            public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width) => throw new NotImplementedException();

            public override ResponseObject<decimal> GetHighestHigh(TradingPair pair, CandleWidth width, int numberOfCandles) => throw new NotImplementedException();

            public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack) => throw new NotImplementedException();
        }
    }
}