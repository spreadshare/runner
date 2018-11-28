using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a state switch event in the database
    /// </summary>
    internal class StateSwitchEvent : ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateSwitchEvent"/> class.
        /// </summary>
        /// <param name="timestamp">timestamp</param>
        /// <param name="from">origin state</param>
        /// <param name="to">goal state</param>
        public StateSwitchEvent(long timestamp, string from, string to)
        {
            Timestamp = timestamp;
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets or sets the unique ID of the event.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the state from which was switched
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the state to which was switched
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Get a header matching the format of the CSV representation
        /// </summary>
        /// <param name="delimiter">delimiter</param>
        /// <returns>csv header</returns>
        public static string GetStaticCsvHeader(char delimiter)
        {
            return $"{nameof(Id)}{delimiter}" +
                   $"{nameof(Timestamp)}{delimiter}" +
                   $"{nameof(From)}{delimiter}" +
                   $"{nameof(To)}";
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
                   $"{From}{delimiter}" +
                   $"{To}";
        }
    }
}