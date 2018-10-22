using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices
{
    /// <summary>
    /// Setup and start exchange factory
    /// </summary>
    public abstract class BaseProviderTests : BaseTest
    {
        /// <summary>
        /// Link to the exchange factory service
        /// </summary>
        internal ExchangeFactoryService ExchangeFactoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public BaseProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            ExchangeFactoryService = serviceProvider.GetService<ExchangeFactoryService>();
            var result = ExchangeFactoryService.Start();
            if (!result.Success)
            {
                Logger.LogError($"Exchange factory service failed to start! {result}");
            }
        }
    }
}