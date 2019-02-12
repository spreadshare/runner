using System;

namespace SpreadShare.Models.Exceptions.OrderExceptions
{
    /// <summary>
    /// Exception thrown when an order has an unexpected order side.
    /// </summary>
    public class UnexpectedOrderSideException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderSideException"/> class.
        /// </summary>
        public UnexpectedOrderSideException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderSideException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public UnexpectedOrderSideException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderSideException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public UnexpectedOrderSideException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}