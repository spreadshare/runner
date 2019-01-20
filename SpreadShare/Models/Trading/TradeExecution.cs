using System;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Model containing information about an executed trade.
    /// </summary>
    internal partial class TradeExecution
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeExecution"/> class.
        /// </summary>
        /// <param name="from">The asset value on the left side of the trade.</param>
        /// <param name="to">The asset value on the right side of the trade.</param>
        public TradeExecution(Balance from, Balance to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the left side of the executed trade.
        /// </summary>
        public Balance From { get; }

        /// <summary>
        /// Gets the right side of the executed trade.
        /// </summary>
        public Balance To { get; }

        /// <summary>
        /// Parse a trade execution from a <see cref="OrderUpdate"/>.
        /// </summary>
        /// <param name="order">OrderUpdate to derive an execution from.</param>
        /// <returns>TradeExecution order derived from the provided OrderUpdate.</returns>
        public static TradeExecution FromOrder(OrderUpdate order)
        {
            switch (order.Status)
            {
                case OrderUpdate.OrderStatus.New:
                    return FromNewOrder(order);
                case OrderUpdate.OrderStatus.PartiallyFilled:
                case OrderUpdate.OrderStatus.Filled:
                    return FromFillOrder(order);
                case OrderUpdate.OrderStatus.Cancelled:
                    return FromCancelledOrder(order);
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), $"{order.Status} cannot be parsed to a TradeExection");
            }
        }
    }
}