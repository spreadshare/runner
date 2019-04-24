using System;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

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

        /// <summary>
        /// Convert CandleWidth to Binance.Net.KlineInterval enum.
        /// </summary>
        /// <param name="width">The width of a candle.</param>
        /// <returns>Binance.Net.KlineInterval.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Only OneMinute and FiveMinutes are currently available.</exception>
        public static KlineInterval ToExternal(int width)
        {
            switch (width)
            {
                case 1:
                    return KlineInterval.OneMinute;
                case 5:
                    return KlineInterval.FiveMinutes;
                default:
                    throw new ArgumentOutOfRangeException(nameof(width), width, $"{width} is not a valid KlineInterval on Binance.");
            }
        }

        /// <summary>
        /// Convert a Binance.Net.Binance.StreamOrderUpdate to a SpreadShare.OrderUpdate.
        /// </summary>
        /// <param name="orderInfoUpdate">Binance.Net.StreamOrderUpdate.</param>
        /// <returns>OrderUpdate.</returns>
        public static OrderUpdate ToInternal(BinanceStreamOrderUpdate orderInfoUpdate)
        {
            // TODO: Trade Id is wrong.
            var order = new OrderUpdate(
                orderId: orderInfoUpdate.OrderId,
                tradeId: 0,
                orderType: ToInternal(orderInfoUpdate.Type),
                orderStatus: ToInternal(orderInfoUpdate.Status),
                createdTimestamp: orderInfoUpdate.OrderCreationTime.ToUnixTimestampMilliseconds(),
                setPrice: orderInfoUpdate.Price,
                side: ToInternal(orderInfoUpdate.Side),
                pair: TradingPair.Parse(orderInfoUpdate.Symbol),
                setQuantity: orderInfoUpdate.Quantity)
            {
                LastFillIncrement = orderInfoUpdate.QuantityOfLastFilledTrade,
                LastFillPrice = orderInfoUpdate.PriceLastFilledTrade,
                AverageFilledPrice = HelperMethods.SafeDiv(
                    orderInfoUpdate.CummulativeQuoteQuantity,
                    orderInfoUpdate.AccumulatedQuantityOfFilledTrades),
                FilledQuantity = orderInfoUpdate.AccumulatedQuantityOfFilledTrades,
                StopPrice = orderInfoUpdate.StopPrice,
            };

            try
            {
                order.Commission = orderInfoUpdate.Commission;
                order.CommissionAsset = new Currency(orderInfoUpdate.CommissionAsset);
            }
            catch (ArgumentException)
            {
                // ignored
            }

            return order;
        }

        /// <summary>
        /// Converts a binance sourced candle to an internal model.
        /// </summary>
        /// <param name="candle">The candle to convert.</param>
        /// <returns><see cref="BacktestingCandle"/> instance.</returns>
        public static BacktestingCandle ToInternal(BinanceStreamKlineData candle)
        {
            return new BacktestingCandle(
                openTimestamp: candle.Data.OpenTime.ToUnixTimestampMilliseconds(),
                open: candle.Data.Open,
                close: candle.Data.Close,
                high: candle.Data.High,
                low: candle.Data.Low,
                volume: candle.Data.Volume,
                tradingPair: candle.Symbol);
        }

        /// <summary>
        /// Convert a number of minutes to a <see cref="KlineInterval"/>.
        /// </summary>
        /// <param name="width">Number of minutes.</param>
        /// <returns>Binance kline interval.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Non compatible intervals.</exception>
        public static KlineInterval ToInternalKline(int width)
        {
            switch (width)
            {
                case 1: return KlineInterval.OneMinute;
                case 5: return KlineInterval.FiveMinutes;
                default: throw new ArgumentOutOfRangeException(nameof(width));
            }
        }

        /// <summary>
        /// Converts an Binance API error code to internal ResponseCode.
        /// </summary>
        /// <param name="errorCode">The code to convert.</param>
        /// <returns>ResponseCode.</returns>
        public static ResponseCode ToInternalError(int errorCode)
        {
            switch (errorCode)
            {
                case -2010: return ResponseCode.ImmediateOrderTrigger;
                default: return ResponseCode.Error;
            }
        }
    }
}