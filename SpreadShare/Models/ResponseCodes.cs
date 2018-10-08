namespace SpreadShare.Models
{
    /// <summary>
    /// Object representation of the code of the response to an action
    /// </summary>
    /// TODO: Should this not be called ResponseStatus?
    internal enum ResponseCodes
    {
        /// <summary>
        /// The action resulted in an error
        /// </summary>
        Error,

        /// <summary>
        /// The action was not defined
        /// </summary>
        NotDefined,

        /// <summary>
        /// The action was successful
        /// </summary>
        Success
    }
}