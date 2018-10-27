namespace SpreadShare.ExchangeServices.ExchangeCommunicationService
{
    /// <summary>
    /// Interface for exchange communications services.
    /// </summary>
    /// <typeparam name="TClient">Type of the client</typeparam>
    internal interface IExchangeCommunicationService<out TClient>
    {
        /// <summary>
        /// Gets the client that commmunicates with the exchange
        /// </summary>
        /// <returns>The client that communicates with the exchange</returns>
        TClient Client { get; }
    }
}