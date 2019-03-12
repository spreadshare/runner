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

        /// <summary>
        /// Gets or sets a value indicating whether this session is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the name of the session.
        /// </summary>
        public string Name { get; set; }
    }
}