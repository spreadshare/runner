using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Defines the session of the current instance.
    /// </summary>
    internal class AlgorithmSession
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

        /// <summary>
        /// Gets or sets the id of the docker container this session was started in.
        /// </summary>
        public string ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp at which the session was created.
        /// </summary>
        public long CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp at which the session was closed.
        /// </summary>
        public long ClosedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a json representation of the current allocation.
        /// </summary>
        public string AllocationJson { get; set; }

        /// <summary>
        /// Gets or sets the portfolio representation of the <see cref="AllocationJson"/> value.
        /// </summary>
        [NotMapped]
        public Portfolio Allocation
        {
            get => JsonConvert.DeserializeObject<Portfolio>(AllocationJson);
            set => AllocationJson = JsonConvert.SerializeObject(value);
        }
    }
}