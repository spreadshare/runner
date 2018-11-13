using System;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices
{
    public class ConfigurableObserverTests : BaseProviderTests
    {
        private readonly DataProvider _data;

        public ConfigurableObserverTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var container =
                ExchangeFactoryService.BuildContainer(Exchange.Backtesting, typeof(SimpleBandWagonAlgorithm));
            _data = container.DataProvider;
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var observer = new ConfigurableObserver<bool>(x => { }, () => { }, e => { });
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(null, () => { }, e => { }));
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(x => { }, null, e => { }));
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(x => { }, () => { }, null));
        }

        [Fact]
        public void SubscribeOrderUpdateHappyFlow()
        {
            var observer = new ConfigurableObserver<OrderUpdate>(
                x => { Logger.LogInformation("Observed trade update"); },
                () => { },
                e => { });
            _data.Subscribe(observer);
        }
    }
}