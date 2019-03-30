using System;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.Providers.Observing;
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
                ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            _data = container.DataProvider;
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var observer = new ConfigurableObserver<bool>(() => { }, _ => { }, _ => { });
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(null, _ => { }, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(() => { }, null, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new ConfigurableObserver<bool>(() => { }, _ => { }, null));
        }
    }
}