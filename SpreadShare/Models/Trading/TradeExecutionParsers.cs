using Dawn;
using SpreadShare.Utilities;
using static SpreadShare.Models.Trading.OrderUpdate.OrderTypes;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Parsing functionality, split from <see cref="TradeExecution"/>.
    /// </summary>
    internal partial class TradeExecution
    {
        /// <summary>
        /// Parse from a new order, redirects either to <see cref="FromNewMarketOrder"/>  or <see cref="FromNewNonMarketOrder"/>.
        /// </summary>
        /// <param name="order">OrderUpdate.</param>
        /// <returns>Parsed trade execution.</returns>
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

            // For market orders, fees are incorporated in the quantity.
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

            var (commission, asset) = order.Commission;
            if (asset != currencyTo)
            {
                commission = order.Side == OrderSide.Buy
                    ? HelperMethods.SafeDiv(commission, order.AverageFilledPrice)
                    : commission * order.AverageFilledPrice;
            }

            // Only subtract commission for buy orders
            var from = new Balance(currencyFrom, quantityFrom, 0M);
            var to = new Balance(currencyTo, quantityTo - commission, 0M);
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

            var from = order.Side == OrderSide.Buy ? order.Pair.Right : order.Pair.Left;
            var to = order.Side == OrderSide.Buy ? order.Pair.Left : order.Pair.Right;

            var (commission, asset) = order.Commission;
            if (asset != to)
            {
                commission = order.Side == OrderSide.Buy
                    ? HelperMethods.SafeDiv(commission, order.AverageFilledPrice)
                    : commission * order.AverageFilledPrice;
            }

            if (order.Side == OrderSide.Buy)
            {
                return new TradeExecution(
                    new Balance(from, 0.0M, order.LastFillIncrement * order.SetPrice),
                    new Balance(to, order.LastFillIncrement - commission, 0.0M));
            }
            else
            {
                return new TradeExecution(
                    new Balance(from, 0, order.LastFillIncrement),
                    new Balance(to, (order.LastFillIncrement * order.LastFillPrice) - commission, 0));
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