using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a state switch event in the database.
    /// </summary>
    internal class StateSwitchEvent : IDatabaseEvent, ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateSwitchEvent"/> class.
        /// </summary>
        /// <param name="eventTimestamp">Timestamp.</param>
        /// <param name="name">Goal state.</param>
        public StateSwitchEvent(long eventTimestamp, string name)
        {
            EventTimestamp = eventTimestamp;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSwitchEvent"/> class.
        /// </summary>
        /// <param name="eventTimestamp">Timestamp.</param>
        /// <param name="name">Goal state.</param>
        /// <param name="session">The current session.</param>
        public StateSwitchEvent(long eventTimestamp, string name, AlgorithmSession session)
        {
            EventTimestamp = eventTimestamp;
            Name = name;
            Session = session;
        }

        /// <summary>
        /// Gets or sets the unique ID of the event.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event.
        /// </summary>
        public long EventTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the state to which was switched.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public AlgorithmSession Session { get; set; }

        /// <summary>
        /// Get a header matching the format of the CSV representation.
        /// </summary>
        /// <param name="delimiter">Delimiter.</param>
        /// <returns>csv header.</returns>
        public static string GetStaticCsvHeader(char delimiter)
        {
            return $"{nameof(Id)}{delimiter}" +
                   $"{nameof(EventTimestamp)}{delimiter}" +
                   $"{nameof(Name)}{delimiter}";
        }

        /// <inheritdoc />
        public string GetCsvHeader(char delimiter)
        {
            return GetStaticCsvHeader(delimiter);
        }

        /// <inheritdoc />
        public string GetCsvRepresentation(char delimiter)
        {
            return $"{Id}{delimiter}" +
                   $"{EventTimestamp}{delimiter}" +
                   $"{Name}{delimiter}";
        }
    }
}