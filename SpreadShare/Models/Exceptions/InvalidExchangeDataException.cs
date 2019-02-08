namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when the data given by the exchange is invalid.
    /// </summary>
    public class InvalidExchangeDataException : ProviderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExchangeDataException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public InvalidExchangeDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExchangeDataException"/> class.
        /// </summary>
        public InvalidExchangeDataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExchangeDataException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public InvalidExchangeDataException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}