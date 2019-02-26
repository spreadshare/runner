using Binance.Net.Interfaces;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.Models.Trading;

namespace SpreadShare.Tests.Stubs.Binance
{
    internal class TestBinanceCommunicationsService : BinanceCommunicationsService
    {
        public TestBinanceCommunicationsService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override IBinanceClient Client => new TestBinanceClient(this);

        public void ScheduleObserverEvent(OrderUpdate order)
        {
            UpdateObservers(order);
        }
    }
}