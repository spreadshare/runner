using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.TradingProviderTests
{
    public class PlaceLimitOrderTests : BaseProviderTests
    {
        public PlaceLimitOrderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
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

        private TradingProvider GetTradingProvider<T>()
            where T : TradingProviderTestImplementation
        {
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            var trading = container.TradingProvider;
            var property = trading.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(TradingProvider)}");

            // Inject test implementation
            var implementation = Activator.CreateInstance(typeof(T), LoggerFactory, container.TimerProvider);
            property.SetValue(trading, implementation);
            return trading;
        }

        private class PlaceLimitOrderHappyFlowImplementation : TradingProviderTestImplementation
        {
            public PlaceLimitOrderHappyFlowImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
            }

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

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
            {
                if (quantity != 337.69M)
                {
                    throw new Exception($"Full limit order did not fetch the correct quantity, expected 337.69M, got {quantity}");
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

        private abstract class TradingProviderTestImplementation : AbstractTradingProvider
        {
            protected TradingProviderTestImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
                Cache = new List<OrderUpdate>();
            }

            protected List<OrderUpdate> Cache { get; }

            public abstract override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId);

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId) => throw new NotImplementedException();

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId) => throw new NotImplementedException();

            public override ResponseObject CancelOrder(TradingPair pair, long orderId) => throw new NotImplementedException();

            public override ResponseObject<OrderUpdate> WaitForOrderStatus(long orderId, OrderUpdate.OrderStatus status)
            {
                foreach (var order in Cache)
                {
                    if (order.OrderId == orderId && order.Status == status)
                    {
                        return new ResponseObject<OrderUpdate>(order);
                    }
                }

                return new ResponseObject<OrderUpdate>(ResponseCode.Error);
            }

            public override ResponseObject<OrderUpdate> GetOrderInfo(long orderId) => throw new NotImplementedException();

            public override void OnCompleted() => throw new NotImplementedException();

            public override void OnError(Exception error) => throw new NotImplementedException();

            public override void OnNext(long value) => throw new NotImplementedException();
        }
    }
}