using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService
{
    /// <summary>
    /// Defines the layout for classes communicating with exchanges.
    /// </summary>
    internal abstract class ExchangeCommunications : Observable<OrderUpdate>
    {
        private bool _connected;

        /// <summary>
        /// Startup the service (if not already started).
        /// </summary>
        public void Connect()
        {
            if (!_connected)
            {
                Startup();
                _connected = true;
            }
        }

        /// <summary>
        /// Routine for starting up the connection.
        /// </summary>
        protected abstract void Startup();
    }
}
