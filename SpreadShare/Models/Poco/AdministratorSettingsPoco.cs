using System.Collections.Generic;

namespace SpreadShare.Models.Poco
{
    /// <summary>
    /// Poco object for direct JSON parsing.
    /// </summary>
    internal sealed class AdministratorSettingsPoco
    {
        /// <summary>
        /// Gets the raw AdminEmail.
        /// </summary>
        public string AdminEmail { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the raw AdminPassword.
        /// </summary>
        public string AdminPassword { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the raw list of Recipients.
        /// </summary>
        public List<string> Recipients { get; private set; } = new List<string>();
    }
}