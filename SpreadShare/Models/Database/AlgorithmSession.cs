using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Defines the session of the current instance.
    /// </summary>
    public class AlgorithmSession
    {
        /// <summary>
        /// Gets or sets the ID of the algorithm session.
        /// </summary>
        [Key]
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the name of the session.
        /// </summary>
        public string Name { get; set; }
    }
}