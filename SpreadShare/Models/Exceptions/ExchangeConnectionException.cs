using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs with the communication with an exchange.
    /// </summary>
    public class ExchangeConnectionException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeConnectionException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public ExchangeConnectionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeConnectionException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public ExchangeConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // ReSharper disable once UnusedMember.Local
        private ExchangeConnectionException()
        {
        }
    }
}