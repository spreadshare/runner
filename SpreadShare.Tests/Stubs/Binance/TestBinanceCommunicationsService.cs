using System.Threading.Tasks;
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

        public async void ScheduleObserverEvent(OrderUpdate order)
        {
            // Wait a small amount of time to ensure that the BinanceTradingProvider has
            // been able to add it's middleware transformation.
            await Task.Delay(2000).ConfigureAwait(false);
            UpdateObservers(order);
        }
    }
}