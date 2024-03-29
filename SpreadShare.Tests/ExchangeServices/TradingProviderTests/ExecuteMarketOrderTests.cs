using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.TradingProviderTests
{
    public class ExecuteMarketOrderTests : TradingProviderTestUtils
    {
        public ExecuteMarketOrderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ExecuteMarketOrderBuyPairNull()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.ExecuteMarketOrderBuy(null, 10));
        }

        [Fact]
        public void ExecuteMarketOrderSellPairNull()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.ExecuteMarketOrderSell(null, 10));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ExecuteMarketOrderBuyQuantityZeroOrNegative(decimal quantity)
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), quantity));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ExecuteMarketOrderSellQuantityZeroOrNegative(decimal quantity)
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                trading.ExecuteMarketOrderSell(TradingPair.Parse("EOSETH"), quantity));
        }

        [Fact]
        public void ExecuteMarketOrderHappyFlow()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10);
        }

        [Fact]
        public void ExecuteMarketOrderWrongSideBuy()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() => trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10));
        }

        [Fact]
        public void ExecuteMarketOrderWrongSideSell()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() =>
                trading.ExecuteMarketOrderSell(TradingPair.Parse("EOSETH"), 10));
        }

        [Fact]
        public void ExecuteMarketOrderWrongType()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderWrongTypeImplementation>();
            Assert.Throws<UnexpectedOrderTypeException>(() =>
                trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10));
        }

        [Fact]
        public void ExecuteMarketOrderNeverFilled()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderNeverFilledImplementation>();
            Assert.Throws<OrderFailedException>(
                () => trading.ExecuteMarketOrderBuy(TradingPair.Parse("EOSETH"), 10));
        }

        [Fact]
        public void ExecuteFullMarketOrderHappyFlow()
        {
            var trading = GetTradingProvider<ExecuteFullMarketOrderHappyFlowImplementation>();
            trading.ExecuteFullMarketOrderSell(TradingPair.Parse("BNBBTC"));
        }

        [Fact]
        public void ExecuteMarketOrderRefused()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            var c1 = new Currency(TestAllocationManager.RefuseCoin);
            var c2 = new Currency("ETH");
            Assert.Throws<OrderRefusedException>(() =>
                trading.ExecuteMarketOrderSell(TradingPair.Parse(c1, c2), 10));
        }

        [Fact]
        public void ExecuteMarketOrderQuantityUnrounded()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderQuantityUnroundedImplementation>();
            trading.ExecuteMarketOrderSell(TradingPair.Parse("EOSETH"), 2832.5223452932M);
        }

        [Fact]
        public void ExecuteMarketOrderPartialBuyHappyFlow()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            trading.ExecutePartialMarketOrderBuy(TradingPair.Parse("EOSETH"), 0.5M);
        }

        [Fact]
        public void ExecuteMarketOrderPartialSellHappyFlow()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            trading.ExecutePartialMarketOrderSell(TradingPair.Parse("EOSETH"), 0.5M);
        }

        [Fact]
        public void ExecuteMarketOrderPartialBuyNotZero()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderBuy(TradingPair.Parse("EOSETH"), 0M));
        }

        [Fact]
        public void ExecuteMarketOrderPartialSellNotZero()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderSell(TradingPair.Parse("EOSETH"), 0M));
        }

        [Fact]
        public void ExecuteMarketOrderPartialBuyNotNegative()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderBuy(TradingPair.Parse("EOSETH"), -1M));
        }

        [Fact]
        public void ExecuteMarketOrderPartialSellNotNegative()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderSell(TradingPair.Parse("EOSETH"), -1M));
        }

        [Fact]
        public void ExecuteMarketOrderPartialBuyExactlyOne()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            trading.ExecutePartialMarketOrderBuy(TradingPair.Parse("EOSETH"), 1M);
        }

        [Fact]
        public void ExecuteMarketOrderPartialSellExactlyOne()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            trading.ExecutePartialMarketOrderSell(TradingPair.Parse("EOSETH"), 1M);
        }

        [Fact]
        public void ExecuteMarketOrderPartialBuyMoreThanOne()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderBuy(TradingPair.Parse("EOSETH"), 1.00000001M));
        }

        [Fact]
        public void ExecuteMarketOrderPartialSellMoreThanOne()
        {
            var trading = GetTradingProvider<ExecuteMarketOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.ExecutePartialMarketOrderSell(TradingPair.Parse("EOSETH"), 1.00000001M));
        }

        // Classes are instantiated via the Activator
        #pragma warning disable CA1812

        private class ExecuteMarketOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public ExecuteMarketOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.Market,
                    createdTimestamp: 0,
                    setPrice: 0,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class ExecuteMarketOrderWrongSideImplementation : TradingProviderTestImplementation
        {
            public ExecuteMarketOrderWrongSideImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                var wrongSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                var order = new OrderUpdate(
                    createdTimestamp: 0,
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.Market,
                    setPrice: 0,
                    side: wrongSide,
                    pair: pair,
                    setQuantity: quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class ExecuteMarketOrderWrongTypeImplementation : TradingProviderTestImplementation
        {
            public ExecuteMarketOrderWrongTypeImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                var order = new OrderUpdate(
                    createdTimestamp: 0,
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.StopLossLimit,
                    setPrice: 0,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class ExecuteMarketOrderNeverFilledImplementation : TradingProviderTestImplementation
        {
            public ExecuteMarketOrderNeverFilledImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                var order = new OrderUpdate(
                    createdTimestamp: 0,
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.StopLossLimit,
                    setPrice: 0,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                // Not added to cache -> cause timeout.
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class ExecuteFullMarketOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public ExecuteFullMarketOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                if (quantity != 337.69M)
                {
                    throw new Exception($"Full limit order did not fetch the correct quantity, expected 337.69M, got {quantity}");
                }

                var order = new OrderUpdate(
                    createdTimestamp: 0,
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.Market,
                    setPrice: 0,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class ExecuteMarketOrderQuantityUnroundedImplementation : TradingProviderTestImplementation
        {
            public ExecuteMarketOrderQuantityUnroundedImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
            {
                if (quantity != 2832.5223452932M)
                {
                    throw new Exception("The TradingProvider should not round quantities");
                }

                var order = new OrderUpdate(
                    createdTimestamp: 0,
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.Filled,
                    orderType: OrderUpdate.OrderTypes.Market,
                    setPrice: 0,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }
        #pragma warning restore CA1812
    }
}