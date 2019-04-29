using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Database entry for logging statements.
    /// </summary>
    internal class LogEvent : IDatabaseEvent
    {
        /// <summary>
        /// Gets or sets the id of the log.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <inheritdoc />
        public long EventTimestamp { get; set; }

        /// <summary>
        /// Gets or sets level of the log.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the text of the log.
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc />
        public AlgorithmSession Session { get; set; }
    }
}