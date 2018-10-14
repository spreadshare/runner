using SpreadShare.Models;

namespace SpreadShare.SupportServices.SettingsService
{
    /// <summary>
    /// Service for managing settings
    /// </summary>
    internal interface ISettingsService
    {
        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns>Whether the service started successfully</returns>
        ResponseObject Start();
    }
}