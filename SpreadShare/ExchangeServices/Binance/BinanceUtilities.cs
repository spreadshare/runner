using System;
using OrderSide = Binance.Net.Objects.OrderSide;

namespace SpreadShare.ExchangeServices.Binance
{
    /// <summary>
    /// Utility methods collection for Binance.Net related subjects
    /// </summary>
    internal static class BinanceUtilities
    {
        /// <summary>
        /// Convert Binance.Net to SpreadShare.Models
        /// </summary>
        /// <param name="side">Binance.Net.Orderside</param>
        /// <returns>SpreadShare.Models.OrderSide</returns>
        public static Models.OrderSide ToInternal(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return Models.OrderSide.Buy;
                case OrderSide.Sell:
                    return Models.OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }

        /// <summary>
        /// Convert SpreadShare.Models to Binance.Net
        /// </summary>
        /// <param name="side">SpreadShare.Models.OrderSide</param>
        /// <returns>Binance.Net.OrderSide</returns>
        public static OrderSide ToExternal(Models.OrderSide side)
        {
            switch (side)
            {
                case Models.OrderSide.Buy:
                    return OrderSide.Buy;
                case Models.OrderSide.Sell:
                    return OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }
    }
}