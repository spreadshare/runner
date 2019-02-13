using System;

namespace SpreadShare.Models.Exceptions.OrderExceptions
{
    /// <summary>
    /// Exception thrown when an order has een unexpected order status.
    /// </summary>
    public class UnexpectedOrderStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderStatusException"/> class.
        /// </summary>
        public UnexpectedOrderStatusException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderStatusException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public UnexpectedOrderStatusException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderStatusException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public UnexpectedOrderStatusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}