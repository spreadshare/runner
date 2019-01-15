using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Tests.ExchangeServices;
using SpreadShare.Tests.Stubs;
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
                .BuildContainer(typeof(TemplateAlgorithm));
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var unused = new TestState();
        }

        [Fact]
        public void RunHappyFlow()
        {
            var state = new TestState();
            state.Activate(new TemplateAlgorithmSettings(), _container, LoggerFactory);
        }

        [Fact]
        public void MarketPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnMarketCondition(_container.DataProvider);
            Assert.IsType<NothingState<TemplateAlgorithmSettings>>(next);
        }

        [Fact]
        public void OrderPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnOrderUpdate(null);
            Assert.IsType<NothingState<TemplateAlgorithmSettings>>(next);
        }
    }
}