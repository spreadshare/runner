using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
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
            : base(loggerFactory, portfolioFetcherService)
        {
        }

        /// <summary>
        /// Make sure that allocation is ignored for tests.
        /// </summary>
        /// <param name="exchange">exchange.</param>
        /// <param name="algo">algo.</param>
        /// <param name="exec">exec.</param>
        public override void UpdateAllocation(Exchange exchange, Type algo, TradeExecution exec) => Expression.Empty();

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Type algorithm, Exchange exchange, Func<OrderUpdate> tradeCallback)
        {
            if (p.From.Symbol == new Currency(RefuseCoin))
            {
                return ResponseCommon.OrderRefused;
            }

            return new ResponseObject<OrderUpdate>(tradeCallback());
        }
    }

    #pragma warning restore CA1812
}