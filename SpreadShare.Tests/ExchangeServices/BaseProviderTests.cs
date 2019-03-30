using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Tests.Stubs;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace SpreadShare.Tests.ExchangeServices
{
    /// <summary>
    /// Setup and start exchange factory.
    /// </summary>
    public abstract class BaseProviderTests : BaseTest
    {
        /// <summary>
        /// Link to the exchange factory service.
        /// </summary>
        internal readonly ExchangeFactoryService ExchangeFactoryService;

        private const string AlgorithmSettingsSource =
        @"
           TradingPairs: [ EOSETH ]
           CandleWidth: 5 
        ";

        private static object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output.</param>
        public BaseProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            ExchangeFactoryService = serviceProvider.GetService<ExchangeFactoryService>();
            AlgorithmConfiguration = new DeserializerBuilder().Build()
                    .Deserialize<TemplateAlgorithmConfiguration>(new StringReader(AlgorithmSettingsSource));
            ConfigurationValidator.ValidateConstraintsRecursively(AlgorithmConfiguration);

            // Ensure that the allocation manager is configured atomically.
            // Tests are run concurrently so the lock is required.
            lock (_lock)
            {
                var alloc = serviceProvider.GetService<AllocationManager>();
                alloc.SetInitialConfiguration(TestPortfolioFetcher.GetStaticPortfolio());
            }
        }

        internal TemplateAlgorithmConfiguration AlgorithmConfiguration { get; }
    }
}