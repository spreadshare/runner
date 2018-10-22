using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
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
        /// Link to the allocation manager
        /// </summary>
        internal WeakAllocationManager AllocationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProviderTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Used to create output</param>
        public BaseProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var fullAllocationService = serviceProvider.GetService<AllocationManager>();

            AllocationManager = fullAllocationService.GetWeakAllocationManager();
            ExchangeFactoryService = serviceProvider.GetService<ExchangeFactoryService>();

            var result = ExchangeFactoryService.Start();
            if (!result.Success)
            {
                Logger.LogError($"Exchange factory service failed to start! {result}");
            }
        }
    }
}