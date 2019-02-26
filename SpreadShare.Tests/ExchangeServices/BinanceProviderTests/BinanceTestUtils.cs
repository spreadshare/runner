using System;
using System.Reflection;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Tests.Stubs.Binance;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    public class BinanceTestUtils : BaseProviderTests
    {
        protected BinanceTestUtils(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        internal TradingProvider GetTradingProvider()
        {
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            var trading = container.TradingProvider;
            var property = trading.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(TradingProvider)}");

            // Inject test communications
            var comms = new TestBinanceCommunicationsService(LoggerFactory);

            // Inject test implementation
            var binance = new BinanceTradingProvider(LoggerFactory, comms, container.TimerProvider);
            property.SetValue(trading, binance);
            return trading;
        }
    }
}