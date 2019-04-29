using System;
using System.Reflection;
using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.ExchangeServices;
using SpreadShare.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.Tests.Algorithms
{
    public class StateTests : BaseProviderTests
    {
        private readonly ExchangeProvidersContainer _container;
        private readonly Action<State<TemplateAlgorithmConfiguration>, TimeSpan> _setTimer;

        public StateTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _container = ExchangeFactoryService.BuildBinanceContainer<TemplateAlgorithm>(AlgorithmConfiguration);

            var setTimerMethod = typeof(State<TemplateAlgorithmConfiguration>)
                .GetMethod("SetTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            _setTimer = (state, timespan) =>
            {
                try
                {
                    setTimerMethod.Invoke(state, new object[] { timespan });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            };
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
            state.Activate(AlgorithmConfiguration, _container);
        }

        [Fact]
        public void MarketPredicateDefaultNothing()
        {
            var state = new TestState();
            var next = state.OnMarketCondition(_container.DataProvider);
            Assert.IsType<NothingState<TemplateAlgorithmConfiguration>>(next);
        }

        [Fact]
        public void OnTimerDefaultNothing()
        {
            var state = new TestState();
            Program.CommandLineArgs.Trading = true;
            state.Activate(AlgorithmConfiguration, _container);
            var next = state.OnTimerElapsed();
            Assert.IsType<NothingState<TemplateAlgorithmConfiguration>>(next);
        }

        [Fact]
        public void OnTimerBacktestDefaultError()
        {
            var state = new TestState();
            Program.CommandLineArgs.Trading = false;
            Assert.Throws<AlgorithmLogicException>(() => state.OnTimerElapsed());
        }

        [Fact]
        public void SetTimerPastThrows()
        {
            var state = new TestState();
            Assert.Throws<ArgumentOutOfRangeException>(() => _setTimer(state, TimeSpan.FromSeconds(-1)));
        }

        [Fact]
        public void SetTimerSetsEndTime()
        {
            var state = new TestState();
            var now = _container.TimerProvider.CurrentTime;
            var span = TimeSpan.FromDays(8);

            state.Activate(AlgorithmConfiguration, _container);
            _setTimer(state, span);
            Assert.Equal((now + span).DateTime, state.EndTime.DateTime, TimeSpan.FromMilliseconds(1000));
        }

        [Fact]
        public void OrderPredicateNullNothing()
        {
            var state = new TestState();
            state.Activate(AlgorithmConfiguration, _container);
            var next = state.OnOrderUpdate(null);
            Assert.IsType<NothingState<TemplateAlgorithmConfiguration>>(next);
        }

        [Fact]
        public void OrderPredicateDefaultNothing()
        {
            var state = new TestState();
            state.Activate(AlgorithmConfiguration, _container);
            Program.CommandLineArgs.Trading = true;
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.Filled,
                orderType: OrderUpdate.OrderTypes.Limit,
                createdTimestamp: 0,
                setPrice: 0,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var next = state.OnOrderUpdate(order);
            Assert.IsType<NothingState<TemplateAlgorithmConfiguration>>(next);
        }

        [Fact]
        public void OrderPredicateDefaultBacktestError()
        {
            var state = new TestState();
            state.Activate(AlgorithmConfiguration, _container);
            Program.CommandLineArgs.Trading = false;
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.Filled,
                orderType: OrderUpdate.OrderTypes.Limit,
                createdTimestamp: 0,
                setPrice: 0,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            Assert.Throws<AlgorithmLogicException>(() => state.OnOrderUpdate(order));
        }

        [Fact]
        public void OrderPredicateMarketNoError()
        {
            var state = new TestState();
            state.Activate(AlgorithmConfiguration, _container);
            Program.CommandLineArgs.Trading = false;
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.Filled,
                orderType: OrderUpdate.OrderTypes.Market,
                createdTimestamp: 0,
                setPrice: 0,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var next = state.OnOrderUpdate(order);
            Assert.IsType<NothingState<TemplateAlgorithmConfiguration>>(next);
        }
    }
}