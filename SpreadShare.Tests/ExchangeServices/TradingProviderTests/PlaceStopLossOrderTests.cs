using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.TradingProviderTests
{
    public class PlaceStopLossOrderTests : TradingProviderTestUtils
    {
        public PlaceStopLossOrderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void PlaceStoplossOrderHappyFlowBuy()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10, 1);
        }

        [Fact]
        public void PlaceStoplossOrderHappyFlowSell()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), 10, 1);
        }

        [Fact]
        public void PlaceFullStoplossOrderHappyFlow()
        {
            var trading = GetTradingProvider<PlaceFullStoplossOrderHappyFlowImplementation>();
            trading.PlaceFullStoplossSell(TradingPair.Parse("BNBBTC"), 5);
        }

        [Fact]
        public void PlaceStoplossOrderBuyPairNull()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.PlaceStoplossBuy(null, 10M, 1M));
        }

        [Fact]
        public void PlaceStoplossOrderSellPairNull()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentNullException>(() => trading.PlaceStoplossSell(null, 10M, 1M));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(10, 0)]
        [InlineData(10, -1)]
        public void PlaceStoplossOrderBuyQuantityPriceZeroOrNegative(decimal quantity, decimal price)
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), quantity, price));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(10, 0)]
        [InlineData(10, -1)]
        public void PlaceStoplossOrderSellQuantityPriceZeroOrNegative(decimal quantity, decimal price)
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), quantity, price));
        }

        [Fact]
        public void PlaceStoplossReportWrongSideBuy()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderReportWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() =>
                trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10M, 1M));
        }

        [Fact]
        public void PlaceStoplossReportWrongSideSell()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderReportWrongSideImplementation>();
            Assert.Throws<UnexpectedOrderSideException>(() =>
                trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), 10M, 1M));
        }

        [Fact]
        public void PlaceStoplossReportWrongTypeBuy()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderReportWrongTypeImplementation>();
            Assert.Throws<UnexpectedOrderTypeException>(() =>
                trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10M, 1M));
        }

        [Fact]
        public void PlaceStoplossReportWrongTypeSell()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderReportWrongTypeImplementation>();
            Assert.Throws<UnexpectedOrderTypeException>(() =>
                trading.PlaceStoplossSell(TradingPair.Parse("EOSETH"), 10M, 1M));
        }

        [Fact]
        public void PlaceStoplossUnroundedBuy()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderUnroundedImplementation>();
            trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 34.2344634238482M, 1M);
        }

        [Fact]
        public void PlaceStoplossUnroundedSell()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderUnroundedImplementation>();
            trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 34.2344634238482M, 1M);
        }

        [Fact]
        public void PlaceStoplossOrderNeverConfirmed()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderNeverConfirmedImplementation>();
            Assert.Throws<ExchangeTimeoutException>(() => trading.PlaceStoplossBuy(TradingPair.Parse("EOSETH"), 10, 1M));
        }

        [Fact]
        public void PlaceStoplossOrderRefused()
        {
            var trading = GetTradingProvider<PlaceStoplossOrderHappyFlowImplementation>();
            var c1 = new Currency(TestAllocationManager.RefuseCoin);
            var c2 = new Currency("ETH");
            Assert.Throws<OrderRefusedException>(() =>
                trading.PlaceStoplossSell(TradingPair.Parse(c1, c2), 10, 1));
        }

        // Classes are instantiated via the Activator
        #pragma warning disable CA1812

        private class PlaceStoplossOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public PlaceStoplossOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.StopLoss,
                    createdTimeStamp: 0,
                    setPrice: price,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceStoplossOrderReportWrongSideImplementation : TradingProviderTestImplementation
        {
            public PlaceStoplossOrderReportWrongSideImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var wrongSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.StopLoss,
                    createdTimeStamp: 0,
                    setPrice: price,
                    side: wrongSide,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceStoplossOrderReportWrongTypeImplementation : TradingProviderTestImplementation
        {
            public PlaceStoplossOrderReportWrongTypeImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.Limit,
                    createdTimeStamp: 0,
                    setPrice: price,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceStoplossOrderUnroundedImplementation : TradingProviderTestImplementation
        {
            public PlaceStoplossOrderUnroundedImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                if (quantity != 34.2344634238482M)
                {
                    throw new Exception("The TradingProvider should not round quantities");
                }

                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.StopLoss,
                    createdTimeStamp: 0,
                    setPrice: price,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                Cache.Add(order);
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceStoplossOrderNeverConfirmedImplementation : TradingProviderTestImplementation
        {
            public PlaceStoplossOrderNeverConfirmedImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.StopLoss,
                    createdTimeStamp: 0,
                    setPrice: price,
                    side: side,
                    pair: pair,
                    setQuantity: quantity);

                // Do not add to cache -> cause timeout
                return new ResponseObject<OrderUpdate>(order);
            }
        }

        private class PlaceFullStoplossOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public PlaceFullStoplossOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

            protected override List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                if (quantity != 337.69M)
                {
                    throw new Exception("Portfolio is not correctly fetched, wrong quantity");
                }

                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.StopLoss,
                    createdTimeStamp: 0,
                    setPrice: price,
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