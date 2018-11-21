using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a state switch event in the database
    /// </summary>
    internal class StateSwitchEvent
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
        /// Get a header matching the format of ToString()
        /// </summary>
        /// <returns>string header</returns>
        public static string GetCsvHeader()
        {
            return $"{nameof(Id)}, " +
                   $"{nameof(Timestamp)}, " +
                   $"{nameof(From)}, " +
                   $"{nameof(To)}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Id}, " +
                   $"{Timestamp}, " +
                   $"{From}, " +
                   $"{To}";
        }
    }
}