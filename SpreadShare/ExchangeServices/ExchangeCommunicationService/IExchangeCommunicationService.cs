using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService
{
    /// <summary>
    /// Interface for exchange communications services.
    /// </summary>
    internal interface IExchangeCommunicationService
    {
        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns>Response object indicating wether the service started successfully</returns>
        ResponseObject Start();
    }
}