using System.Collections.Generic;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings for enable status of services
    /// </summary>
    internal class EnabledServices
    {
        /// <summary>
        /// Gets or sets a value indicating whether algorithm service is enabled
        /// </summary>
        public Dictionary<string, bool> Algorithms { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether trading service is enabled
        /// </summary>
        public bool Trading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user service is enabled
        /// </summary>
        public bool User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ZeroMQ service is enabled
        /// </summary>
        public bool ZeroMq { get; set; }
    }
}