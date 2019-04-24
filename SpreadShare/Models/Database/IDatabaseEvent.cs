namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Abstract definition of an event.
    /// </summary>
    internal interface IDatabaseEvent
    {
        /// <summary>
        /// Gets or sets the session that this event is related to.
        /// </summary>
        AlgorithmSession Session { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event.
        /// </summary>
        long EventTimestamp { get; set; }
}
}