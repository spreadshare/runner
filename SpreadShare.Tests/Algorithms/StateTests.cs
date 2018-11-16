using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.Tests.ExchangeServices;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Algorithms
{
    public class StateTests : BaseProviderTests
    {
        private TradingProvider _trading;
        private DataProvider _data;

        public StateTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var container = ExchangeFactoryService
                .BuildContainer(Exchange.Backtesting, typeof(SimpleBandWagonAlgorithmSettings));
            _trading = container.TradingProvider;
            _data = container.DataProvider;
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
            state.Activate(new TestSettings() { Value = 1 }, _trading, _data, LoggerFactory);
        }

        [Fact]
        public void MarketPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnMarketCondition(_data);
            Assert.IsType<NothingState<TestSettings>>(next);
        }

        [Fact]
        public void OrderPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnOrderUpdate(null);
            Assert.IsType<NothingState<TestSettings>>(next);
        }

        internal class TestState : State<TestSettings>
        {
            protected override void Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation($"Running, value is {AlgorithmSettings.Value}");
            }
        }

        internal class TestSettings : AlgorithmSettings
        {
            public override Exchange Exchange { get; set; }

            public decimal Value { get; set; }
        }
    }
}