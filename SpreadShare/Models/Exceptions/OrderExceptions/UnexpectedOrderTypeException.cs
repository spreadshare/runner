using System;

namespace SpreadShare.Models.Exceptions.OrderExceptions
{
    /// <summary>
    /// Exception thrown when an order has an unexpected type.
    /// </summary>
    public class UnexpectedOrderTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderTypeException"/> class.
        /// </summary>
        public UnexpectedOrderTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderTypeException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public UnexpectedOrderTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedOrderTypeException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public UnexpectedOrderTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}