using System.Threading;
using Binance.Net.Interfaces;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;

namespace SpreadShare.Tests.Stubs.Binance
{
    internal class TestBinanceCommunicationsService : BinanceCommunicationsService
    {
        public TestBinanceCommunicationsService(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Client = new TestBinanceClient(this);
        }

        public sealed override IBinanceClient Client { get; protected set; }

        public void ScheduleObserverEvent(OrderUpdate order)
        {
            new Thread(() => UpdateObservers(order)).Start();
        }
    }
}