using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a certain action is not permitted.
    /// </summary>
    public class PermissionDeniedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public PermissionDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        public PermissionDeniedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public PermissionDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}