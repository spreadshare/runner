using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when an email address is invalid.
    /// </summary>
    public class InvalidEmailException : InvalidConfigurationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public InvalidEmailException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailException"/> class.
        /// </summary>
        public InvalidEmailException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEmailException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public InvalidEmailException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}