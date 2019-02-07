using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using Xunit;
using Xunit.Abstractions;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    public class AllocationManagerTests : BaseTest
    {
        private readonly IPortfolioFetcherService _fetcher;
        private readonly BacktestCommunicationService _comms;

        public AllocationManagerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _fetcher = serviceProvider.GetService<IPortfolioFetcherService>();
            _comms = serviceProvider.GetService<BacktestCommunicationService>();
        }

        private static IEnumerable<Balance> SortedSettingsBalances =>
            Configuration.Instance.BacktestSettings.Portfolio.AllBalances().OrderByDescending(x => x.Free);

        [Fact]
        public void ConstructorHappyFlow()
        {
            var unused = new AllocationManager(LoggerFactory, _fetcher);
        }

        [Fact]
        public void ValidateAllocationsSetRequired()
        {
            var allocationManager = new AllocationManager(LoggerFactory, _fetcher);

            Currency c = new Currency("ETH");

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAvailableFunds(
                Exchange.Backtesting,
                typeof(TemplateAlgorithm),
                c));

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAllFunds(
                Exchange.Backtesting,
                typeof(TemplateAlgorithm)));

            Assert.Throws<ArgumentNullException>(() => allocationManager.QueueTrade(
                new TradeProposal(TradingPair.Parse("EOSETH"), new Balance(c, 10, 10)),
                typeof(TemplateAlgorithm),
                Exchange.Backtesting,
                () => new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.Limit,
                    createdTimeStamp: 0,
                    setPrice: 0,
                    side: OrderSide.Buy,
                    pair: TradingPair.Parse("EOSETH"),
                    setQuantity: 0)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0.001)]
        public void SetAllocationHappyFlow(decimal factor)
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher);
            alloc.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
            {
                {
                  Exchange.Backtesting,
                  new Dictionary<Type, decimal>
                  {
                     { typeof(TemplateAlgorithm), factor },
                  }
                },
            });

            Currency c = new Currency("ETH");
            var local = Configuration.Instance.BacktestSettings.Portfolio.GetAllocation(c);
            var amount = alloc.GetAvailableFunds(Exchange.Backtesting, typeof(TemplateAlgorithm), c);
            Assert.Equal(local.Free * factor, amount.Free);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-0.0001)]
        [InlineData(2.5)]
        public void SetAllocationInvalidFactor(decimal factor)
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher);
            Assert.Throws<ArgumentException>(() => alloc.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
            {
                {
                    Exchange.Backtesting,
                    new Dictionary<Type, decimal>
                    {
                        { typeof(TemplateAlgorithm), factor },
                    }
                },
            }));
        }

        [Fact]
        public void QueueTradeHappyFlow()
        {
            Type algo = typeof(TemplateAlgorithm);
            var alloc = MakeDefaultAllocation();

            var weak = alloc.GetWeakAllocationManager(algo, Exchange.Backtesting);

            // Get most valuable asset from backtesting settings.
            decimal total = weak.GetAvailableFunds(new Currency("ETH")).Free;
            decimal quote = weak.GetAvailableFunds(new Currency("EOS")).Free;
            Balance balance = new Balance(new Currency("ETH"), total, 0);

            var proposal = new TradeProposal(TradingPair.Parse("EOSETH"), balance);

            var result = weak.QueueTrade(proposal, () =>
            {
                Logger.LogInformation($"Trading all of the {proposal.From.Free}{proposal.From.Symbol}");

                // Mock real world side effects by changing the 'remote' portfolio
                var order = new OrderUpdate(
                    orderId: 0,
                    tradeId: 0,
                    orderStatus: OrderUpdate.OrderStatus.New,
                    orderType: OrderUpdate.OrderTypes.Market,
                    createdTimeStamp: 0,
                    setPrice: 1,
                    side: OrderSide.Buy,
                    pair: TradingPair.Parse("EOSETH"),
                    setQuantity: proposal.From.Free)
                {
                    AverageFilledPrice = 1,
                    FilledQuantity = proposal.From.Free,
                    LastFillPrice = 1,
                    LastFillIncrement = proposal.From.Free,
                };
                _comms.RemotePortfolio.UpdateAllocation(TradeExecution.FromOrder(order));
                return order;
            });

            Assert.True(result.Success, "Valid trade was declared invalid");
            Assert.Equal(total - proposal.From.Free, weak.GetAvailableFunds(balance.Symbol).Free);
            Assert.Equal(quote + proposal.From.Free, weak.GetAvailableFunds(new Currency("EOS")).Free);
        }

        [Fact]
        public void QueueTradeInvalid()
        {
            Type algo = typeof(TemplateAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            Balance balance = alloc.GetAvailableFunds(new Currency("ETH"));
            var proposal = new TradeProposal(TradingPair.Parse("EOSETH"), new Balance(
                balance.Symbol,
                balance.Free + 1,
                balance.Locked));

            var result = alloc.QueueTrade(proposal, () =>
            {
                Logger.LogCritical("Invalid trade is being executed");
                Assert.True(false, "Trade callback of invalid trade is being executed");
                return null;
            });

            Assert.False(result.Success, "Invalid proposal was reported as executed");
        }

        [Fact]
        public void QueueTradeNull()
        {
            Type algo = typeof(TemplateAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            Assert.Throws<ArgumentNullException>(() => alloc.QueueTrade(null, () => null));
        }

        [Fact]
        public void QueueTradeReportZero()
        {
            Type algo = typeof(TemplateAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            Balance balance = alloc.GetAvailableFunds(new Currency("ETH"));
            var proposal = new TradeProposal(TradingPair.Parse("EOSETH"), balance);

            var result = alloc.QueueTrade(proposal, () =>
                new OrderUpdate(
                    0,
                    0,
                    OrderUpdate.OrderStatus.Filled,
                    OrderUpdate.OrderTypes.Limit,
                    0,
                    0,
                    OrderSide.Buy,
                    TradingPair.Parse("EOSETH"),
                    0));
            Assert.True(result.Success, "Valid proposal was not executed");

            // Assert that the allocation was not mutated
            Assert.Equal(proposal.From.Free, alloc.GetAvailableFunds(proposal.From.Symbol).Free);
            Assert.Equal(proposal.From.Locked, alloc.GetAvailableFunds(proposal.From.Symbol).Locked);
        }

        [Fact]
        public void GetAllFundsHappyFlow()
        {
            Type algo = typeof(TemplateAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            var funds = alloc.GetAllFunds();
            Assert.NotNull(funds);
            Assert.Equal(SortedSettingsBalances.Count(), funds.AllBalances().Count());
        }

        [Fact]
        public void GetAllFundEmpty()
        {
            Type algo = typeof(TemplateAlgorithm);
            var totalalloc = new AllocationManager(LoggerFactory, _fetcher);
            totalalloc.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
            {
                { Exchange.Backtesting, new Dictionary<Type, decimal>() },
            });
            var alloc = totalalloc.GetWeakAllocationManager(algo, Exchange.Backtesting);
            var funds = alloc.GetAllFunds();
            Assert.NotNull(funds);
            Assert.Empty(funds.AllBalances());
        }

        private AllocationManager MakeDefaultAllocation(decimal scale = 1.0M)
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher);
            alloc.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
            {
                {
                    Exchange.Backtesting,
                    new Dictionary<Type, decimal>
                    {
                        { typeof(TemplateAlgorithm), scale },
                    }
                },
            });

            // Free up at least 10 ETH
            alloc.UpdateAllocation(
                Exchange.Backtesting,
                typeof(TemplateAlgorithm),
                new TradeExecution(
                    Balance.Empty(new Currency("ETH")),
                    new Balance(new Currency("ETH"), 10, 0)));
            return alloc;
        }
    }
}