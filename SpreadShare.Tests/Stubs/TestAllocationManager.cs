using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;

namespace SpreadShare.Tests.Stubs
{
    // This class is instantiated via the ServiceProvider.
    #pragma warning disable CA1812

    internal class TestAllocationManager : AllocationManager
    {
        public const string RefuseCoin = "VIA";

        public TestAllocationManager(ILoggerFactory loggerFactory, IPortfolioFetcherService portfolioFetcherService)
            : base(loggerFactory, portfolioFetcherService, null)
        {
        }

        /// <summary>
        /// Makes sure that allocation is ignored for tests.
        /// </summary>
        /// <param name="exec">exec.</param>
        public override void UpdateAllocation(TradeExecution exec) => Expression.Empty();

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Func<OrderUpdate> tradeCallback)
        {
            if (p.From.Symbol == new Currency(RefuseCoin))
            {
                return ResponseObject.OrderRefused;
            }

            return new ResponseObject<OrderUpdate>(tradeCallback());
        }
    }

    #pragma warning restore CA1812
}