using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Defines the order verifier extension for order updates.
    /// </summary>
    internal static class OrderVerifierExtension
    {
        /// <summary>
        /// Get an order verifier instance.
        /// </summary>
        /// <param name="order">The order to verify.</param>
        /// <returns>an order verifier.</returns>
        public static OrderVerifier Verify(this OrderUpdate order)
        {
            return new OrderVerifier(order);
        }

        /// <summary>
        /// Exposes a chain of predicates to impose on an order.
        /// </summary>
        internal class OrderVerifier
        {
            private OrderUpdate _order;

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderVerifier"/> class.
            /// </summary>
            /// <param name="order">The order to verify.</param>
            public OrderVerifier(OrderUpdate order)
            {
                _order = order;
            }

            /// <summary>
            /// Imposes that an order is buy order.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsBuy() => IsSide(OrderSide.Buy);

            /// <summary>
            /// Impose that an order is a sell order.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsSell() => IsSide(OrderSide.Sell);

            /// <summary>
            /// Impose that an order has status new.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsNew() => HasStatus(OrderUpdate.OrderStatus.New);

            /// <summary>
            /// Impose that an order has status filled.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsFilled() => HasStatus(OrderUpdate.OrderStatus.Filled);

            /// <summary>
            /// Impose that an order has status cancelled.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsCancelled() => HasStatus(OrderUpdate.OrderStatus.Cancelled);

            /// <summary>
            /// Imposes that an order has type limit.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsMarket() => IsType(OrderUpdate.OrderTypes.Market);

            /// <summary>
            /// Imposes that an order has type limit.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsStopLoss() => IsType(OrderUpdate.OrderTypes.StopLoss);

            /// <summary>
            /// Imposes that an order has type limit.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsStopLossLimit() => IsType(OrderUpdate.OrderTypes.StopLossLimit);

            /// <summary>
            /// Imposes that an order has type limit.
            /// </summary>
            /// <returns>OrderVerifier.</returns>
            public OrderVerifier IsLimit() => IsType(OrderUpdate.OrderTypes.Limit);

            private OrderVerifier IsType(OrderUpdate.OrderTypes orderType)
            {
                return _order.OrderType == orderType
                    ? this
                    : throw new UnexpectedOrderTypeException(
                        $"Order {_order.OrderId} has unexpect order type, expected {orderType} got {_order.OrderType}");
            }

            private OrderVerifier HasStatus(OrderUpdate.OrderStatus status)
            {
                return _order.Status == status
                    ? this
                    : throw new UnexpectedOrderStatusException(
                        $"Order {_order.OrderId} has unexpected status, expected {status} got {_order.Status}");
            }

            private OrderVerifier IsSide(OrderSide side)
            {
                return _order.Side == side
                    ? this
                    : throw new UnexpectedOrderSideException(
                        $"Order {_order.OrderId} has unexpected side, expected {side}, got {_order.Side}");
            }
        }
    }
}