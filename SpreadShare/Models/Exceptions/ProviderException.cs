using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown by ExchangeProviders.
    /// </summary>
    public abstract class ProviderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        protected ProviderException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        protected ProviderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        protected ProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}