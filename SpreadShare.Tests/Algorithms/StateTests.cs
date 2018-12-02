using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.Tests.ExchangeServices;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Algorithms
{
    public class StateTests : BaseProviderTests
    {
        private readonly ExchangeProvidersContainer _container;

        public StateTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _container = ExchangeFactoryService
                .BuildContainer(typeof(SimpleBandWagonAlgorithm));
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var state = new TestState();
        }

        [Fact]
        public void RunHappyFlow()
        {
            var state = new TestState();
            state.Activate(new TestSettings() { Value = 1 }, _container, LoggerFactory);
        }

        [Fact]
        public void MarketPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnMarketCondition(_container.DataProvider);
            Assert.IsType<NothingState<TestSettings>>(next);
        }

        [Fact]
        public void OrderPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnOrderUpdate(null);
            Assert.IsType<NothingState<TestSettings>>(next);
        }

        private class TestState : State<TestSettings>
        {
            protected override void Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation($"Running, value is {AlgorithmSettings.Value}");
            }
        }

        private class TestSettings : AlgorithmSettings
        {
            public decimal Value { get; set; }
        }
    }
}