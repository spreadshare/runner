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
    public class PlaceLimitOrderTests : TradingProviderTestUtils
    {
        public PlaceLimitOrderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void PlaceLimitOrderBuyPairNull()
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.PlaceLimitOrderBuy(null, 10, 1));
        }

        [Fact]
        public void PlaceLimitOrderSellPairNull()
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.PlaceLimitOrderSell(null, 10, 1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(10, 0)]
        [InlineData(10, -1)]
        public void PlaceLimitOrderBuyQuantityPriceZeroOrNegative(decimal quantity, decimal price)
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), quantity, price));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(10, 0)]
        [InlineData(10, -1)]
        public void PlaceLimitOrderSellQuantityPriceZeroOrNegative(decimal quantity, decimal price)
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), quantity, price));
        }

        [Fact]
        public void PlaceLimitOrderHappyFlow()
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 1);
        }

        [Fact]
        public void PlaceLimitReportOrderWrongSideBuy()
        {
            var trading = GetTradingProvider<PlaceLimitOrderReportWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() => trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 1));
        }

        [Fact]
        public void PlaceLimitReportOrderWrongSideSell()
        {
            var trading = GetTradingProvider<PlaceLimitOrderReportWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() => trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), 10, 1));
        }

        [Fact]
        public void PlaceLimitOrderWrongTypeBuy()
        {
            var trading = GetTradingProvider<PlaceLimitOrderWrongTypeImplementation>();
            Assert.Throws<UnexpectedOrderTypeException>(() =>
                trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 1));
        }

        [Fact]
        public void PlaceLimitOrderWrongTypeSell()
        {
            var trading = GetTradingProvider<PlaceLimitOrderWrongTypeImplementation>();
            Assert.Throws<UnexpectedOrderTypeException>(() =>
                trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), 10, 1));
        }

        [Fact]
        public void PlaceFullLimitOrderHappyFlow()
        {
            var trading = GetTradingProvider<PlaceFullLimitOrderHappyFlowImplementation>();
            trading.PlaceFullLimitOrderSell(TradingPair.Parse("BNBBTC"), 5);
        }

        [Fact]
        public void PlaceLimitOrderNeverConfirmed()
        {
            var trading = GetTradingProvider<PlaceLimitOrderNeverConfirmedImplementation>();
            Assert.Throws<OrderFailedException>(
                () => trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 10, 1));
        }

        [Fact]
        public void PlaceLimitOrderUnroundedBuy()
        {
            var trading = GetTradingProvider<PlaceLimitOrderUnroundedImplementation>();
            trading.PlaceLimitOrderBuy(TradingPair.Parse("EOSETH"), 3.2384932482723M, 1M);
        }

        [Fact]
        public void PlaceLimitOrderUnroundedSell()
        {
            var trading = GetTradingProvider<PlaceLimitOrderUnroundedImplementation>();
            trading.PlaceLimitOrderSell(TradingPair.Parse("EOSETH"), 3.2384932482723M, 1M);
        }

        [Fact]
        public void PlaceLimitOrderRefused()
        {
            var trading = GetTradingProvider<PlaceLimitOrderHappyFlowImplementation>();
            var c1 = new Currency(TestAllocationManager.RefuseCoin);
            var c2 = new Currency("ETH");
            Assert.Throws<OrderRefusedException>(() =>
                trading.PlaceLimitOrderSell(TradingPair.Parse(c1, c2), 10, 1));
        }

        // Classes are instantiated via the Activator
        #pragma warning disable CA1812

        private class PlaceLimitOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Limit,
                    0,
                    price,
                    side,
                    pair,
                    quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceLimitOrderReportWrongSideImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderReportWrongSideImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var wrongSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Limit,
                    0,
                    price,
                    wrongSide,
                    pair,
                    quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceFullLimitOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public PlaceFullLimitOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var target = 337.69M;
                if (quantity != target)
                {
                    throw new Exception($"Full limit order did not fetch the correct quantity, expected {target}, got {quantity}");
                }

                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Limit,
                    0,
                    price,
                    side,
                    pair,
                    quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceLimitOrderWrongTypeImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderWrongTypeImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Market,
                    0,
                    price,
                    side,
                    pair,
                    quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceLimitOrderNeverConfirmedImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderNeverConfirmedImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Market,
                    0,
                    price,
                    side,
                    pair,
                    quantity);

                // Do not add to cache -> cause timeout.
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceLimitOrderUnroundedImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderUnroundedImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                if (quantity != 3.2384932482723M)
                {
                    throw new Exception($"The TradingProvider should not round quantities");
                }

                var order = new OrderUpdate(
                    0,
                    tradeId,
                    OrderUpdate.OrderStatus.New,
                    OrderUpdate.OrderTypes.Limit,
                    0,
                    price,
                    side,
                    pair,
                    quantity);
                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        #pragma warning restore CA1812
    }
}