using SpreadShare.ExchangeServices.Binance;
using SpreadShare.Models;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceTests
{
    /// <summary>
    /// Test collection for the Binance utilities
    /// </summary>
    public class BinanceUtilitiesTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceUtilitiesTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public BinanceUtilitiesTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests conversion from internal to binance objects.
        /// </summary>
        /// <param name="side">SpreadShare.Models order side</param>
        [Theory]
        [InlineData(OrderSide.Buy)]
        [InlineData(OrderSide.Sell)]
        public void ToBinanceOrderConversion(OrderSide side)
        {
            Binance.Net.Objects.OrderSide converted = BinanceUtilities.ToExternal(side);
            Binance.Net.Objects.OrderSide check = side == OrderSide.Buy
                ? Binance.Net.Objects.OrderSide.Buy
                : Binance.Net.Objects.OrderSide.Sell;
            Assert.Equal(converted, check);
        }

        /// <summary>
        /// Tests conversion from binance objects to internal
        /// </summary>
        /// <param name="side">Binance.Net order side</param>
        [Theory]
        [InlineData(Binance.Net.Objects.OrderSide.Buy)]
        [InlineData(Binance.Net.Objects.OrderSide.Sell)]
        public void ToInternalOrderConversion(Binance.Net.Objects.OrderSide side)
        {
            OrderSide converted = BinanceUtilities.ToInternal(side);
            Assert.Equal(converted, side == Binance.Net.Objects.OrderSide.Buy ? OrderSide.Buy : OrderSide.Sell);
        }
    }
}