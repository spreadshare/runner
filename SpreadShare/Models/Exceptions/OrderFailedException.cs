using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when an order is not accepted by the exchange.
    /// </summary>
    public class OrderFailedException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderFailedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public OrderFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderFailedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public OrderFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // ReSharper disable once UnusedMember.Local
        private OrderFailedException()
        {
        }
    }
}