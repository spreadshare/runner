using System;

namespace SpreadShare.Models.Exceptions.OrderExceptions
{
    /// <summary>
    /// Exception thrown when no funds are available.
    /// </summary>
    public class OutOfFundsException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfFundsException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public OutOfFundsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfFundsException"/> class.
        /// </summary>
        public OutOfFundsException()
            : base(ResponseObject.OutOfFunds.Message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfFundsException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public OutOfFundsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}