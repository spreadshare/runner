namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity does not respond in time.
    /// </summary>
    public class ExchangeTimeoutException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimeoutException"/> class.
        /// </summary>
        public ExchangeTimeoutException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimeoutException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public ExchangeTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeTimeoutException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public ExchangeTimeoutException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}