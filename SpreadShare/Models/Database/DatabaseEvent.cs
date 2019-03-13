namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Abstract definition of an event.
    /// </summary>
    public abstract class DatabaseEvent
    {
        /// <summary>
        /// Gets or sets the session that this event is related to.
        /// </summary>
        public AlgorithmSession Session { get; set; }
    }
}