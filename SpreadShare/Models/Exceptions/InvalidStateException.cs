using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Indicates that different values inside the program indicated conflicting truths/intend.
    /// </summary>
    public class InvalidStateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public InvalidStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        public InvalidStateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public InvalidStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}