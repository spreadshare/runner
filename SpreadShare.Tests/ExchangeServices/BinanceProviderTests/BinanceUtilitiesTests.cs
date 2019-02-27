using System;
using Binance.Net.Objects;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    /// <summary>
    /// Test collection for the Binance utilities.
    /// </summary>
    public class BinanceUtilitiesTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceUtilitiesTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public BinanceUtilitiesTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests conversion from internal to binance objects.
        /// </summary>
        /// <param name="side">SpreadShare.Models order side.</param>
        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        public void ToExternalOrderSideConversion(OrderSide side)
        {
            Binance.Net.Objects.OrderSide converted = BinanceUtilities.ToExternal(side);
            Binance.Net.Objects.OrderSide check = side == OrderSide.Buy
                ? Binance.Net.Objects.OrderSide.Buy
                : Binance.Net.Objects.OrderSide.Sell;
            Assert.Equal(converted, check);
        }

        /// <summary>
        /// Tests conversion from binance objects to internal.
        /// </summary>
        /// <param name="side">Binance.Net order side.</param>
        [Theory]
        [InlineData(Binance.Net.Objects.OrderSide.Buy)]
        [InlineData(Binance.Net.Objects.OrderSide.Sell)]
        public void ToInternalOrderSideConversion(Binance.Net.Objects.OrderSide side)
        {
            OrderSide converted = BinanceUtilities.ToInternal(side);
            Assert.Equal(converted, side == Binance.Net.Objects.OrderSide.Buy ? OrderSide.Buy : OrderSide.Sell);
        }

        [Fact]
        public void ToInternalOrderUpdateConversionCreationTime()
        {
            var input = new BinanceStreamOrderUpdate
            {
                Symbol = "TRXETH",
                OrderCreationTime = DateTime.UtcNow,
                Side = Binance.Net.Objects.OrderSide.Buy,
            };

            var order = BinanceUtilities.ToInternal(input);
            var diff = DateTimeOffset.Now - DateTimeOffset.FromUnixTimeMilliseconds(order.CreatedTimeStamp);
            Assert.True(diff.TotalSeconds < 1.0, "CreatedTimeStamp of order not parsed correctly.");
        }

        [Fact]
        public void ToInternalOrderUpdateConversionCommision()
        {
            var input = new BinanceStreamOrderUpdate
            {
                Symbol = "TRXETH",
                OrderCreationTime = DateTime.UtcNow,
                Commission = 0.08M,
                CommissionAsset = "EOS",
                Side = Binance.Net.Objects.OrderSide.Buy,
            };

            var order = BinanceUtilities.ToInternal(input);
            Assert.Equal(OrderSide.Buy, OrderSide.Buy);
            Assert.Equal(0.08M, order.Commission);
            Assert.Equal(new Currency("EOS"), order.CommissionAsset);
            Assert.Equal(
                Math.Floor(DateTimeOffset.FromFileTime(DateTime.Now.ToFileTimeUtc()).ToUnixTimeMilliseconds() / 1000.0),
                Math.Floor(order.CreatedTimeStamp / 1000.0));
        }

        [Fact]
        public void ToInternalOrderUpdateConversionNoCommission()
        {
            var input = new BinanceStreamOrderUpdate
            {
                Symbol = "TRXETH",
                OrderCreationTime = DateTime.UtcNow,
                CummulativeQuoteQuantity = 0,
                ExecutionType = ExecutionType.Trade,
            };

            var order = BinanceUtilities.ToInternal(input);
            Assert.Equal(0.0M, order.Commission);
            Assert.Null(order.CommissionAsset);
        }

        [Fact]
        public void ToInternalOrderUpdateDividingQuantities()
        {
            var input = new BinanceStreamOrderUpdate
            {
                Symbol = "TRXETH",
                OrderCreationTime = DateTime.UtcNow,
                CummulativeQuoteQuantity = 6,
                AccumulatedQuantityOfFilledTrades = 10,
                Side = Binance.Net.Objects.OrderSide.Buy,
            };

            var order = BinanceUtilities.ToInternal(input);
            Assert.Equal(6M / 10M, order.AverageFilledPrice);
        }

        [Fact]
        public void ToInternalOrderUpdateDividingZeroSafe()
        {
            var input = new BinanceStreamOrderUpdate
            {
                Symbol = "TRXETH",
                AccumulatedQuantityOfFilledTrades = 0,
                OrderCreationTime = DateTime.UtcNow,
                CummulativeQuoteQuantity = 6,
                Side = Binance.Net.Objects.OrderSide.Buy,
            };

            var order = BinanceUtilities.ToInternal(input);
            Assert.Equal(0M, order.AverageFilledPrice);
        }
    }
}