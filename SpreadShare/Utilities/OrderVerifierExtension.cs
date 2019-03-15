using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using static SpreadShare.Models.Trading.OrderUpdate;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Defines the order verifier extension for order updates.
    /// </summary>
    internal static class OrderVerifierExtension
    {
        /// <summary>
        /// Imposes that an order is buy order.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsBuy(this OrderUpdate o) => IsSide(o, OrderSide.Buy);

        /// <summary>
        /// Impose that an order is a sell order.
        /// </summary>
        /// <returns>OrderVerifier.</returns>
        /// <param name="o">The OrderUpdate instance.</param>
        public static OrderUpdate IsSell(this OrderUpdate o) => IsSide(o, OrderSide.Sell);

        /// <summary>
        /// Impose that an order has status new.
        /// </summary>
        /// <returns>OrderVerifier.</returns>
        /// <param name="o">The OrderUpdate instance.</param>
        public static OrderUpdate IsNew(this OrderUpdate o) => HasStatus(o, OrderStatus.New);

        /// <summary>
        /// Impose that an order has status filled.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsFilled(this OrderUpdate o) => HasStatus(o, OrderStatus.Filled);

        /// <summary>
        /// Impose that an order has status cancelled.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsCancelled(this OrderUpdate o) => HasStatus(o, OrderStatus.Cancelled);

        /// <summary>
        /// Imposes that an order has type limit.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsMarket(this OrderUpdate o) => IsType(o, OrderTypes.Market);

        /// <summary>
        /// Imposes that an order has type limit.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsStopLoss(this OrderUpdate o) => IsType(o, OrderTypes.StopLoss);

        /// <summary>
        /// Imposes that an order has type limit.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsStopLossLimit(this OrderUpdate o) => IsType(o, OrderTypes.StopLossLimit);

        /// <summary>
        /// Imposes that an order has type limit.
        /// </summary>
        /// <param name="o">The OrderUpdate instance.</param>
        /// <returns>OrderVerifier.</returns>
        public static OrderUpdate IsLimit(this OrderUpdate o) => IsType(o, OrderTypes.Limit);

        private static OrderUpdate IsSide(this OrderUpdate order, OrderSide side)
            => order.Side == side
                ? order
                : throw new UnexpectedOrderSideException(
                    $"Order {order.OrderId} has unexpected side, expected {side}, got {order.Side}");

        private static OrderUpdate IsType(this OrderUpdate order, OrderTypes orderType)
            => order.OrderType == orderType
                ? order
                : throw new UnexpectedOrderTypeException(
                    $"Order {order.OrderId} has unexpected order type, expected {orderType} got {order.OrderType}");

        private static OrderUpdate HasStatus(this OrderUpdate order, OrderStatus status)
            => order.Status == status
                ? order
                : throw new UnexpectedOrderStatusException(
                    $"Order {order.OrderId} has unexpected status, expected {status} got {order.Status}");
    }
}