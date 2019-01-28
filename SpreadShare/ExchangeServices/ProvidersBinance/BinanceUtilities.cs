using System;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Utility methods collection for Binance.Net related subjects.
    /// </summary>
    internal static class BinanceUtilities
    {
        /// <summary>
        /// Retry a Binance CallResult method a number of times.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to write errors to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <returns>First (if any) success response of <see pref="method"/>.</returns>
        public static ResponseObject<T> RetryMethod<T>(Func<CallResult<T>> method, ILogger logger, int maxRetries = 5)
        {
            return HelperMethods.RetryMethod(
            () =>
            {
                var result = method();
                return result.Success
                    ? new ResponseObject<T>(ResponseCode.Success, result.Data)
                    : new ResponseObject<T>(ResponseCode.Error, result.Error.Message);
            },
            logger,
            maxRetries);
        }

        /// <summary>
        /// Convert Binance.Net to SpreadShare.Models.
        /// </summary>
        /// <param name="side">Binance.Net.Orderside.</param>
        /// <returns>SpreadShare.Models.OrderSide.</returns>
        public static OrderSide ToInternal(Binance.Net.Objects.OrderSide side)
        {
            switch (side)
            {
                case Binance.Net.Objects.OrderSide.Buy:
                    return OrderSide.Buy;
                case Binance.Net.Objects.OrderSide.Sell:
                    return OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }

        /// <summary>
        /// Convert SpreadShare.Models to Binance.Net.
        /// </summary>
        /// <param name="side">SpreadShare.Models.OrderSide.</param>
        /// <returns>Binance.Net.OrderSide.</returns>
        public static Binance.Net.Objects.OrderSide ToExternal(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return Binance.Net.Objects.OrderSide.Buy;
                case OrderSide.Sell:
                    return Binance.Net.Objects.OrderSide.Sell;
                default:
                    throw new ArgumentException($"{side} not a known order side");
            }
        }

        /// <summary>
        /// Covert Binance.Net to SpreadShare.Models.
        /// </summary>
        /// <param name="status">Binance.Net.OrderStatus.</param>
        /// <returns>SpreadShare.Models.OrderUpdate.OrderStatus.</returns>
        public static OrderUpdate.OrderStatus ToInternal(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.New:
                    return OrderUpdate.OrderStatus.New;
                case OrderStatus.PartiallyFilled:
                    return OrderUpdate.OrderStatus.PartiallyFilled;
                case OrderStatus.Filled:
                    return OrderUpdate.OrderStatus.Filled;
                case OrderStatus.Canceled:
                    return OrderUpdate.OrderStatus.Cancelled;
                case OrderStatus.PendingCancel:
                    return OrderUpdate.OrderStatus.Cancelled;
                case OrderStatus.Rejected:
                    return OrderUpdate.OrderStatus.Rejected;
                case OrderStatus.Expired:
                    return OrderUpdate.OrderStatus.Expired;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        /// <summary>
        /// Convert Binance.Net.OrderTypes to internal enum.
        /// </summary>
        /// <param name="type">Binance.Net.OrderType.</param>
        /// <returns>parsed for internal usage.</returns>
        public static OrderUpdate.OrderTypes ToInternal(OrderType type)
        {
            switch (type)
            {
                case OrderType.Limit:
                    return OrderUpdate.OrderTypes.Limit;
                case OrderType.Market:
                    return OrderUpdate.OrderTypes.Market;
                case OrderType.StopLoss:
                    return OrderUpdate.OrderTypes.StopLoss;
                case OrderType.StopLossLimit:
                    return OrderUpdate.OrderTypes.StopLossLimit;
                case OrderType.TakeProfit:
                    return OrderUpdate.OrderTypes.TakeProfit;
                case OrderType.TakeProfitLimit:
                    return OrderUpdate.OrderTypes.TakeProfitLimit;
                case OrderType.LimitMaker:
                    return OrderUpdate.OrderTypes.LimitMaker;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}