namespace SpreadShare.Models
{
    /// <summary>
    /// Object representation of the code of the response to an action.
    /// </summary>
    internal enum ResponseCode
    {
        /// <summary>
        /// The action resulted in an error
        /// </summary>
        Error,

        /// <summary>
        /// The action was successful
        /// </summary>
        Success,

        /// <summary>
        /// The requested information was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// A StopLoss order was placed at or exceeding market price, causing it to trigger instantly, which is not allowed.
        /// </summary>
        ImmediateOrderTrigger,
    }
}