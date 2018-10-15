using SpreadShare.Models;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Service for managing settings
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns>Whether the service started successfully</returns>
        ResponseObject Start();
    }
}