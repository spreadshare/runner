using System;
using System.Linq.Expressions;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.Tests.Stubs
{
    internal class TestAllocationManager : IAllocationManager
    {
        public const string RefuseCoin = "VIA";
        private readonly TestPortfolioFetcher _portfolio;

        public TestAllocationManager()
        {
            _portfolio = new TestPortfolioFetcher();
        }

        /// <inheritdoc />
        public void SetInitialConfiguration(Portfolio initialAllocation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Portfolio GetAllFunds()
            => _portfolio.GetPortfolio().Data;

        /// <inheritdoc />
        public Balance GetAvailableFunds(Currency currency)
            => GetAllFunds().GetAllocation(currency);

        /// <summary>
        /// Makes sure that allocation is ignored for tests.
        /// </summary>
        /// <param name="exec">exec.</param>
        public void UpdateAllocation(TradeExecution exec) => Expression.Empty();

        /// <inheritdoc />
        public ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Func<OrderUpdate> tradeCallback)
        {
            if (p.From.Symbol == new Currency(RefuseCoin))
            {
                return ResponseObject.OrderRefused;
            }

            return new ResponseObject<OrderUpdate>(tradeCallback());
        }

        public IDisposable Subscribe(IObserver<Portfolio> observer)
        {
            throw new NotImplementedException();
        }
    }
}