using System;
using System.Reflection;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models.Database;
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
                .GetMethod("EvaluateFilledOrder", BindingFlags.NonPublic | BindingFlags.Static);
            GetFilledOrder = (order, candle, time) =>
            {
                try
                {
                    return (bool)method.Invoke(null, new object[] { order, candle, time });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            };
        }

        private Func<OrderUpdate, BacktestingCandle, long, bool> GetFilledOrder { get; }

        [Fact]
        public void GetFilledOrderNullOrder()
        {
            var candle = new BacktestingCandle(0, 1, 1, 1, 1, 1, "EOSETH");
            var filled = GetFilledOrder(null, candle, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledOrderNullCandle()
        {
            var order = GetSomeOrder(OrderTypes.Limit);
            Assert.Throws<ArgumentNullException>(() => GetFilledOrder(order, null, 0));
        }

        [Fact]
        public void GetFilledOrderMarketThrows()
        {
            var order = GetSomeOrder(OrderTypes.Market);
            var candle = new BacktestingCandle(1, 1, 1, 1, 1, 1, "EOSETH");
            Assert.Throws<UnexpectedOrderTypeException>(() =>
            {
                try
                {
                    GetFilledOrder(order, candle, 0);
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
            var candle = new BacktestingCandle(0, 2, 5, 6, 1.8M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledBuyLimitOrderUsesMax()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 6.6M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var candle = new BacktestingCandle(0, 2, 5, 6.6M, 1.8M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
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
            var candle = new BacktestingCandle(0, 4, 3.8M, 3.7M, 4M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.False(filled);
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
                setPrice: 3.8M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var candle = new BacktestingCandle(0, 2, 3.9M, 4.5M, 1.8M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
        }

        [Fact]
        public void GetFilledSellLimitOrderUsedMin()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderTypes.Limit,
                createdTimeStamp: 0,
                setPrice: 1.8M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 0);
            var candle = new BacktestingCandle(0, 2, 3.9M, 4.5M, 1.8M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
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
            var candle = new BacktestingCandle(0, 1, 2, 3.2M, 0.8M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledBuyLimitOrderUnMutated()
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
            var candle = new BacktestingCandle(0, 5, 6, 100M, 0M, 0, "EOSETH");
            GetFilledOrder(order, candle, 2394923);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.Limit, order.OrderType);
            Assert.Equal(2.1M, order.AverageFilledPrice);
            Assert.Equal(2.1M, order.LastFillPrice);
            Assert.Equal(15.6M, order.FilledQuantity);
            Assert.Equal(15.6M, order.LastFillIncrement);
            Assert.Equal(2394923, order.FilledTimeStamp);
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
            var candle = new BacktestingCandle(0, 3, 5, 6.6M, 3M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
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

            var candle = new BacktestingCandle(0, 3, 5, 6.6M, 3M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
            Assert.Equal(3.4M, order.AverageFilledPrice);
        }

        [Fact]
        public void GetFilledSellStoplossOrderUsesMin()
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

            var candle = new BacktestingCandle(0, 5, 4, 6.6M, 3M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
            Assert.Equal(3.4M, order.AverageFilledPrice);
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

            var candle = new BacktestingCandle(0, 4, 5M, 6M, 3M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledBuyStoplossOrderUsesMax()
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

            var candle = new BacktestingCandle(0, 4, 5.5M, 8.2M, 3M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.True(filled);
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
            var candle = new BacktestingCandle(0, 10, 11.2M, 12.4M, 10.1M, 0, "EOSETH");
            var filled = GetFilledOrder(order, candle, 0);
            Assert.False(filled);
        }

        [Fact]
        public void GetFilledBuyStoplossOrderUnMutated()
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

            var candle = new BacktestingCandle(0, 1, 1, 2, 1, 0, "EOSETH");
            GetFilledOrder(order, candle, 434424233);
            Assert.Equal(OrderStatus.Filled, order.Status);
            Assert.Equal(OrderTypes.StopLoss, order.OrderType);
            Assert.Equal(1.9M, order.AverageFilledPrice);
            Assert.Equal(1.9M, order.LastFillPrice);
            Assert.Equal(18.6M, order.FilledQuantity);
            Assert.Equal(18.6M, order.LastFillIncrement);
            Assert.Equal(434424233, order.FilledTimeStamp);
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