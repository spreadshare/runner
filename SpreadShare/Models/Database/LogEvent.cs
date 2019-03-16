using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Database entry for logging statements.
    /// </summary>
    internal class LogEvent : DatabaseEvent
    {
        /// <summary>
        /// Gets or sets the id of the log.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the log.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets level of the log.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the text of the log.
        /// </summary>
        public string Text { get; set; }
    }
}