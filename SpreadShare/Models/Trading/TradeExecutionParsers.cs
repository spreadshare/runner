using Dawn;
using static SpreadShare.Models.Trading.OrderUpdate.OrderTypes;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// <see cref="TradeExecution"/>.
    /// </summary>
    internal partial class TradeExecution
    {
        private static TradeExecution FromNewOrder(OrderUpdate order) =>
            order.OrderType == Market
                ? FromNewMarketOrder(order)
                : FromNewNonMarketOrder(order);

        /// <summary>
        /// Parse from a new, non-market order.
        /// </summary>
        /// <param name="order">OrderUpdate.</param>
        private static TradeExecution FromNewNonMarketOrder(OrderUpdate order)
        {
            var currency = order.Side == OrderSide.Buy ? order.Pair.Right : order.Pair.Left;
            var quantity = order.SetQuantity - order.FilledQuantity;
            quantity = order.Side == OrderSide.Buy ? quantity * order.SetPrice : quantity;
            var free = new Balance(currency, quantity, 0M);
            var locked = new Balance(currency, 0M, quantity);
            return new TradeExecution(free, locked);
        }

        /// <summary>
        /// Parse from a new, market order.
        /// This method assumes the order is filled by definition.
        /// </summary>
        /// <param name="order">OrderUpdate.</param>
        private static TradeExecution FromNewMarketOrder(OrderUpdate order)
        {
            var currencyFrom = order.Side == OrderSide.Buy ? order.Pair.Right : order.Pair.Left;
            var currencyTo = order.Side == OrderSide.Buy ? order.Pair.Left : order.Pair.Right;
            var quantityFrom = order.FilledQuantity * (order.Side == OrderSide.Buy ? order.AverageFilledPrice : 1M);
            var quantityTo = order.FilledQuantity * (order.Side == OrderSide.Sell ? order.AverageFilledPrice : 1M);
            var from = new Balance(currencyFrom, quantityFrom, 0M);
            var to = new Balance(currencyTo, quantityTo - order.Commission, 0M);
            return new TradeExecution(from, to);
        }

        /// <summary>
        /// Parse from a fill type order, locks the assets.
        /// </summary>
        /// <param name="order">OrderUpdate.</param>
        private static TradeExecution FromFillOrder(OrderUpdate order)
        {
            // Some market orders are declared filled right away, redirect their parsing
            if (order.OrderType == Market)
            {
                return FromNewMarketOrder(order);
            }

            // Only subtract the commission when the entire order is filled.
            decimal commission = order.Status == OrderUpdate.OrderStatus.Filled ? order.Commission : 0;

            if (order.Side == OrderSide.Buy)
            {
                return new TradeExecution(
                    new Balance(order.Pair.Right, 0.0M, order.LastFillIncrement * order.SetPrice),
                    new Balance(order.Pair.Left, order.LastFillIncrement - commission, 0.0M));
            }
            else
            {
                return new TradeExecution(
                    new Balance(order.Pair.Left, 0, order.LastFillIncrement),
                    new Balance(order.Pair.Right, (order.LastFillIncrement * order.LastFillPrice) - commission, 0));
            }
        }

        /// <summary>
        /// Parse from cancelled orders, frees the assets.
        /// </summary>
        /// <param name="order">OrderUpdate.</param>
        private static TradeExecution FromCancelledOrder(OrderUpdate order)
        {
            Guard.Argument(order.OrderType).Require(
                x => x != Market,
                x => $"{x} orders cannot be cancelled");

            var currency = order.Side == OrderSide.Buy ? order.Pair.Right : order.Pair.Left;
            var quantity = order.SetQuantity - order.FilledQuantity;
            quantity = order.Side == OrderSide.Buy ? quantity * order.SetPrice : quantity;
            var free = new Balance(currency, quantity, 0M);
            var locked = new Balance(currency, 0M, quantity);
            return new TradeExecution(locked, free);
        }
    }
}