using System;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;
using static SpreadShare.Models.Trading.OrderUpdate.OrderStatus;
using static SpreadShare.Models.Trading.OrderUpdate.OrderTypes;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.Tests.Models
{
    public class TradeExecutionTests : BaseTest
    {
        public TradeExecutionTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        internal void ParseFromMarketBuyOrderNewHappyFlow()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.4M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 40M)
            {
                FilledQuantity = 40M,
                AverageFilledPrice = 0.401M,
            };

            var exec = TradeExecution.FromOrder(order);
            Assert.Equal(order.Pair.Right, exec.From.Symbol);
            Assert.Equal(order.Pair.Left, exec.To.Symbol);
            Assert.Equal(40M * 0.401M, exec.From.Free);
            Assert.Equal(0, exec.From.Locked);
            Assert.Equal(40M, exec.To.Free);
            Assert.Equal(0, exec.To.Locked);
        }

        [Fact]
        internal void ParseFromMarketSellOrderNewHappyFlow()
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.4M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 40M)
            {
                FilledQuantity = 40M,
                AverageFilledPrice = 0.401M,
            };

            var exec = TradeExecution.FromOrder(order);
            Assert.Equal(order.Pair.Left, exec.From.Symbol);
            Assert.Equal(order.Pair.Right, exec.To.Symbol);
            Assert.Equal(40M, exec.From.Free);
            Assert.Equal(0, exec.From.Locked);
            Assert.Equal(40M * 0.401M, exec.To.Free);
            Assert.Equal(0, exec.To.Locked);
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromMarketOrderPriceZero(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.4M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 40M)
            {
                AverageFilledPrice = 0M,
                FilledQuantity = 10M,
            };

            var exec = TradeExecution.FromOrder(order);
            Assert.Equal(exec.From.Locked, decimal.Zero);

            if (side == OrderSide.Sell)
            {
                Assert.Equal(10M, exec.From.Free);
                Assert.Equal(decimal.Zero, exec.To.Free);
            }
            else
            {
                Assert.Equal(decimal.Zero, exec.From.Free);
                Assert.Equal(10M, exec.To.Free);
            }

            Assert.Equal(exec.To.Locked, decimal.Zero);
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromMarketOrderQuantityZero(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.4M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 40M)
            {
                AverageFilledPrice = 0.3M,
                FilledQuantity = 0M,
            };

            var exec = TradeExecution.FromOrder(order);
            Assert.Equal(exec.From.Free, decimal.Zero);
            Assert.Equal(exec.From.Locked, decimal.Zero);
            Assert.Equal(exec.To.Free, decimal.Zero);
            Assert.Equal(exec.To.Locked, decimal.Zero);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketBuyOrderNewHappyFlow(OrderUpdate.OrderTypes orderType)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: orderType,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            var exec = TradeExecution.FromOrder(order);

            Assert.NotNull(exec);

            var symbol = new Currency("ETH");
            Assert.Equal(symbol, exec.From.Symbol);
            Assert.Equal(symbol, exec.To.Symbol);
            Assert.Equal(20M, exec.From.Free);
            Assert.Equal(0M, exec.From.Locked);
            Assert.Equal(0M, exec.To.Free);
            Assert.Equal(20M, exec.To.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketSellOrderNewHappyFlow(OrderUpdate.OrderTypes orderType)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: orderType,
                orderStatus: New,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            var exec = TradeExecution.FromOrder(order);

            Assert.NotNull(exec);

            var symbol = new Currency("EOS");
            Assert.Equal(symbol, exec.From.Symbol);
            Assert.Equal(symbol, exec.To.Symbol);
            Assert.Equal(100M, exec.From.Free);
            Assert.Equal(0M, exec.From.Locked);
            Assert.Equal(0M, exec.To.Free);
            Assert.Equal(100M, exec.To.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketBuyOrderCancelledHappyFlow(OrderUpdate.OrderTypes orderType)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: orderType,
                orderStatus: Cancelled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            var exec = TradeExecution.FromOrder(order);

            Assert.NotNull(exec);

            var symbol = new Currency("ETH");
            Assert.Equal(symbol, exec.From.Symbol);
            Assert.Equal(symbol, exec.To.Symbol);
            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(20M, exec.From.Locked);
            Assert.Equal(20M, exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketSellOrderCancelledHappyFlow(OrderUpdate.OrderTypes orderType)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: orderType,
                orderStatus: Cancelled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            var exec = TradeExecution.FromOrder(order);

            Assert.NotNull(exec);

            var symbol = new Currency("EOS");
            Assert.Equal(symbol, exec.From.Symbol);
            Assert.Equal(symbol, exec.To.Symbol);
            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(100M, exec.From.Locked);
            Assert.Equal(100M, exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromMarketCancelledOrderInvalid(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: Cancelled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            Assert.Throws<ArgumentException>(() => TradeExecution.FromOrder(order));
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromMarketFilled(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M);
            var exec = TradeExecution.FromOrder(order);

            // Assert that the order is calculated as a complete market order
            Assert.NotEqual(exec.From.Symbol, exec.To.Symbol);
            Assert.Equal(0M, exec.From.Locked);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromLimitFilledDifferentPrice(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Limit,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
            };

            var exec = TradeExecution.FromOrder(order);

            Assert.Equal(0M, exec.From.Free);
            if (side == OrderSide.Buy)
            {
                // Allocation was locked using the set price, and should be freed as such
                Assert.Equal(100M * 0.2M, exec.From.Locked);
                Assert.Equal(100M, exec.To.Free);
            }
            else
            {
                Assert.Equal(100M, exec.From.Locked);
                Assert.Equal(100M * 0.15M, exec.To.Free);
            }

            Assert.Equal(0, exec.To.Locked);
        }

        [Theory]
        [InlineData(OrderSide.Sell)]
        [InlineData(OrderSide.Buy)]
        internal void ParseFromMarketOrderCommission(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: Market,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 2.3M,
                CommissionAsset = new Currency("EOS"),
            };

            var exec = TradeExecution.FromOrder(order);

            if (side == OrderSide.Buy)
            {
                Assert.Equal(100M * 0.15M, exec.From.Free);
                Assert.Equal(100M - 2.3M, exec.To.Free);
            }
            else
            {
                Assert.Equal(100M, exec.From.Free);
                Assert.Equal((100M * 0.15M) - (2.3M * 0.15M), exec.To.Free);
            }

            Assert.Equal(0, exec.To.Locked);
            Assert.Equal(0, exec.From.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketBuyOrderCommission(OrderUpdate.OrderTypes type)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: type,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 2.6M,
                CommissionAsset = new Currency("EOS"),
            };

            var exec = TradeExecution.FromOrder(order);

            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(100M * 0.2M, exec.From.Locked);
            Assert.Equal(100M - 2.6M, exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketSellOrderCommission(OrderUpdate.OrderTypes type)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: type,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 0.7M,
                CommissionAsset = new Currency("EOS"),
            };

            var exec = TradeExecution.FromOrder(order);

            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(100M, exec.From.Locked);
            Assert.Equal((100M * 0.15M) - (0.7M * 0.15M), exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        internal void ParseFromMarketOrderForeignCommission(OrderSide side)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: OrderUpdate.OrderTypes.Market,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: side,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 42.69M,
                CommissionAsset = new Currency("BNB"),
            };

            var exec = TradeExecution.FromOrder(order);

            if (side == OrderSide.Buy)
            {
                Assert.Equal(100M * 0.15M, exec.From.Free);
                Assert.Equal(100M, exec.To.Free);
            }
            else
            {
                Assert.Equal(100M, exec.From.Free);
                Assert.Equal(100M * 0.15M, exec.To.Free);
            }

            Assert.Equal(0, exec.To.Locked);
            Assert.Equal(0, exec.From.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketSellOrderForeignCommission(OrderUpdate.OrderTypes type)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: type,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Sell,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 42M,
                CommissionAsset = new Currency("BNB"),
            };

            var exec = TradeExecution.FromOrder(order);

            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(100M, exec.From.Locked);
            Assert.Equal(100M * 0.15M, exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }

        [Theory]
        [InlineData(Limit)]
        [InlineData(StopLoss)]
        [InlineData(StopLossLimit)]
        internal void ParseFromNonMarketBuyOrderForeignCommission(OrderUpdate.OrderTypes type)
        {
            var order = new OrderUpdate(
                orderId: 0,
                tradeId: 0,
                orderType: type,
                orderStatus: Filled,
                createdTimeStamp: 0,
                setPrice: 0.2M,
                side: OrderSide.Buy,
                pair: TradingPair.Parse("EOSETH"),
                setQuantity: 100M)
            {
                FilledQuantity = 100M,
                AverageFilledPrice = 0.15M,
                LastFillIncrement = 100M,
                LastFillPrice = 0.15M,
                Commission = 96.42M,
                CommissionAsset = new Currency("BNB"),
            };

            var exec = TradeExecution.FromOrder(order);

            Assert.Equal(0M, exec.From.Free);
            Assert.Equal(100M * 0.2M, exec.From.Locked);
            Assert.Equal(100M, exec.To.Free);
            Assert.Equal(0M, exec.To.Locked);
        }
    }
}