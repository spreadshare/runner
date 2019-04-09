namespace SpreadShare.Utilities
{
    /// <summary>
    /// Context object for the retry method.
    /// </summary>
    public class RetryContext
    {
        /// <summary>
        /// Gets or sets the current iteration of the retry method.
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// Gets a value indicating whether the backoff should be used.
        /// </summary>
        public bool BackoffDisabled { get; private set; }

        /// <summary>
        /// Sets the <see cref="BackoffDisabled"/> property to true.
        /// </summary>
        public void DisableBackoff() => BackoffDisabled = true;

        /// <summary>
        /// Sets the <see cref="BackoffDisabled"/> property to false.
        /// </summary>
        public void EnableBackoff() => BackoffDisabled = false;
    }
}