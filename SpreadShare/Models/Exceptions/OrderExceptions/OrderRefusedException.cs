using System;
using SpreadShare.Utilities;

namespace SpreadShare.Models.Exceptions.OrderExceptions
{
    /// <summary>
    /// Exception thrown when a trade is refused the allocation manager.
    /// </summary>
    public class OrderRefusedException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRefusedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public OrderRefusedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRefusedException"/> class.
        /// </summary>
        public OrderRefusedException()
            : base(ResponseCommon.OrderRefused.Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRefusedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public OrderRefusedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}