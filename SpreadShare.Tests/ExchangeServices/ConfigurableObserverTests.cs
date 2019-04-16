using System;
using SpreadShare.ExchangeServices.Providers.Observing;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices
{
    public class ConfigurableObserverTests
    {
        public ConfigurableObserverTests()
        {
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var unused = new ConfigurableObserver<bool>(() => { }, _ => { }, _ => { });
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