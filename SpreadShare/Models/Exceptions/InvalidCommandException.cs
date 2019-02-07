using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when an invalid command is encountered.
    /// </summary>
    public class InvalidCommandException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCommandException"/> class.
        /// </summary>
        public InvalidCommandException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCommandException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public InvalidCommandException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCommandException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public InvalidCommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}