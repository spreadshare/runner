using System;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception that has a specific <see cref="ExitCode"/> associated with it.
    /// </summary>
    public abstract class ExitCodeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitCodeException"/> class.
        /// </summary>
        public ExitCodeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitCodeException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public ExitCodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitCodeException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public ExitCodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the exit code associated with this exception.
        /// </summary>
        public abstract ExitCode ExitCode { get; }
    }
}