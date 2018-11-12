using SpreadShare.ExchangeServices.Providers.Observing;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService
{
    /// <summary>
    /// Defines the layout for classes communicating with exchanges.
    /// </summary>
    internal abstract class ExchangeCommunications : Observable<OrderUpdate>
    {
    }
}
