using System;
using System.Reflection;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;
using static SpreadShare.Models.Trading.OrderUpdate;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.Tests.ExchangeServices.BacktestProviderTests
{
    public class BacktestOrderFilledTests : BaseTest
    {
        public BacktestOrderFilledTests(ITestOutputHelper logger)
            : base(logger)
        {
            var method = typeof(BacktestTradingProvider)
                .GetMethod("GetFilledOrder", BindingFlags.NonPublic | BindingFlags.Static);
            GetFilledOrder = (order, price, time) =>
                (bool)method.Invoke(null, new object[] { order, price, time });
        }

        private Func<OrderUpdate, decimal, long, bool> GetFilledOrder { get; }

        [Fact]
        public void GetFilledOrderNull()
        {
            var filled = GetFilledOrder(null, 0, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledOrderMarketThrows()
        {
            var order = GetSomeOrder(OrderTypes.Market);
            Assert.Throws<UnexpectedOrderTypeException>(() =>
            {
                try
                {
                    GetFilledOrder(order, 0, 0);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [Fact]
        public void GetFilledBuyLimitOrderIsFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 3.4M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var filled = GetFilledOrder(order, 3.3M, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledSellLimitOrderIsFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 3.4M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var filled = GetFilledOrder(order, 3.5M, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledBuyLimitOrderIsNotFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 3.4M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var filled = GetFilledOrder(order, 3.6M, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledSellLimitOrderIsNotFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 3.4M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var filled = GetFilledOrder(order, 3.2M, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledBuyLimitOrderHasAttributes()
        {
            var order = new OrderUpdate(
                orderId: 9,
                tradeId: 8,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 302,
                setPrice: 2.1M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("TRXETH"),
                setQuantity: 15.6M);
            GetFilledOrder(order, 2.0M, 2394923);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.Limit, order.OrderType);
            Assert.Equal(2.1M, order.AverageFilledPrice);
            Assert.Equal(2.1M, order.LastFillPrice);
            Assert.Equal(15.6M, order.FilledQuantity);
            Assert.Equal(15.6M, order.LastFillIncrement);
            Assert.Equal(2394923, order.FilledTimeStamp);
        }

        [Fact]
        public void GetFilledSellLimitOrderHasAttributes()
        {
            var order = new OrderUpdate(
                orderId: 9,
                tradeId: 8,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 302,
                setPrice: 2.1M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("TRXETH"),
                setQuantity: 15.6M);
            GetFilledOrder(order, 2.2M, 23423524);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.Limit, order.OrderType);
            Assert.Equal(2.1M, order.AverageFilledPrice);
            Assert.Equal(2.1M, order.LastFillPrice);
            Assert.Equal(15.6M, order.FilledQuantity);
            Assert.Equal(15.6M, order.LastFillIncrement);
            Assert.Equal(23423524, order.FilledTimeStamp);
        }

        [Fact]
        public void GetFilledBuyStoplossOrderIsFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 0,
                setPrice: 0M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0)
            {
                StopPrice = 3.2M,
            };
            var filled = GetFilledOrder(order, 3.3M, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledSellStoplossOrderIsFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 0,
                setPrice: 0,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0)
            {
                StopPrice = 3.4M,
            };

            var filled = GetFilledOrder(order, 3.2M, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledBuyStoplossOrderIsNotFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 0,
                setPrice: 100M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0)
            {
                StopPrice = 6.7M,
            };
            var filled = GetFilledOrder(order, 6.6M, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledSellStoplossOrderIsNotFilled()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 0,
                setPrice: 0M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0)
            {
                StopPrice = 8.5M,
            };
            var filled = GetFilledOrder(order, 9.6M, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledBuyStoplossOrderHasAttributes()
        {
            var order = new OrderUpdate(
                orderId: 9,
                tradeId: 8,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 302,
                setPrice: 2.1M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("TRXETH"),
                setQuantity: 18.6M)
            {
                StopPrice = 1.9M,
            };

            GetFilledOrder(order, 2.0M, 434424233);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.StopLoss, order.OrderType);
            Assert.Equal(1.9M, order.AverageFilledPrice);
            Assert.Equal(1.9M, order.LastFillPrice);
            Assert.Equal(18.6M, order.FilledQuantity);
            Assert.Equal(18.6M, order.LastFillIncrement);
            Assert.Equal(434424233, order.FilledTimeStamp);
        }

        [Fact]
        public void GetFilledSellStoplossOrderHasAttributes()
        {
            var order = new OrderUpdate(
                orderId: 9,
                tradeId: 8,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.StopLoss,
                createdTimeStamp: 302,
                setPrice: 2.1M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("TRXETH"),
                setQuantity: 12.6M)
            {
                StopPrice = 5.01M,
            };

            GetFilledOrder(order, 4.9M, 234235952);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.StopLoss, order.OrderType);
            Assert.Equal(5.01M, order.AverageFilledPrice);
            Assert.Equal(5.01M, order.LastFillPrice);
            Assert.Equal(12.6M, order.FilledQuantity);
            Assert.Equal(12.6M, order.LastFillIncrement);
            Assert.Equal(234235952, order.FilledTimeStamp);
        }

        private static OrderUpdate GetSomeOrder(OrderTypes type, decimal setPrice = 0)
        {
            return new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.Filled,
                orderType: type,
                createdTimeStamp: 0,
                setPrice: setPrice,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
        }
    }
}