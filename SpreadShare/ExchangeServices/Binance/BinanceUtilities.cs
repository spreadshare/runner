using System;
using SpreadShare.Models;

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
        public static OrderSide ToInternal(global::Binance.Net.Objects.OrderSide side)
        {
            switch (side)
            {
                case global::Binance.Net.Objects.OrderSide.Buy:
                    return OrderSide.Buy;
                case global::Binance.Net.Objects.OrderSide.Sell:
                    return OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }

        /// <summary>
        /// Convert SpreadShare.Models to Binance.Net
        /// </summary>
        /// <param name="side">SpreadShare.Models.OrderSide</param>
        /// <returns>Binance.Net.OrderSide</returns>
        public static global::Binance.Net.Objects.OrderSide ToExternal(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return global::Binance.Net.Objects.OrderSide.Buy;
                case OrderSide.Sell:
                    return global::Binance.Net.Objects.OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }
    }
}