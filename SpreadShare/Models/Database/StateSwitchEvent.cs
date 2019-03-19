using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a state switch event in the database.
    /// </summary>
    internal class StateSwitchEvent : DatabaseEvent, ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateSwitchEvent"/> class.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        /// <param name="name">Goal state.</param>
        public StateSwitchEvent(long timestamp, string name)
        {
            Timestamp = timestamp;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSwitchEvent"/> class.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        /// <param name="name">Goal state.</param>
        /// <param name="session">The current session.</param>
        public StateSwitchEvent(long timestamp, string name, AlgorithmSession session)
        {
            Timestamp = timestamp;
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
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the state to which was switched.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get a header matching the format of the CSV representation.
        /// </summary>
        /// <param name="delimiter">Delimiter.</param>
        /// <returns>csv header.</returns>
        public static string GetStaticCsvHeader(char delimiter)
        {
            return $"{nameof(Id)}{delimiter}" +
                   $"{nameof(Timestamp)}{delimiter}" +
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
                   $"{Timestamp}{delimiter}" +
                   $"{Name}{delimiter}";
        }
    }
}