using System;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.Providers.Observing;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices
{
    /// <inheritdoc />
    /// <summary>
    /// Tests regadring the timer provider.
    /// </summary>
    public class TimerProviderTests : BaseProviderTests
    {
        private readonly TimerProvider _time;

        public TimerProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var container =
                ExchangeFactoryService.BuildContainer(Exchange.Backtesting, typeof(SimpleBandWagonAlgorithm));
            _time = container.TimerProvider;
        }

        [Fact]
        public void NoCallbackThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _time.SetTimer(0, null));
        }

        [Fact]
        public void SubscribeObserverHappyFlow()
        {
            var observer = new ConfigurableObserver<long>(x => { }, () => { }, e => { });
            _time.Subscribe(observer);
        }
    }
}