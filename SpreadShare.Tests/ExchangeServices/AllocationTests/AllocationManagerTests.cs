using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

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
            SettingsService.BacktestInitialPortfolio.AllBalances().OrderByDescending(x => x.Free);

        [Fact]
        public void ConstructorHappyFlow()
        {
            var allocationManager = new AllocationManager(LoggerFactory, _fetcher);
        }

        [Fact]
        public void ValidateAllocationsSetRequired()
        {
            var allocationManager = new AllocationManager(LoggerFactory, _fetcher);

            Currency c = new Currency("ETH");

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAvailableFunds(
                Exchange.Backtesting,
                typeof(SimpleBandWagonAlgorithm),
                c));

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAllFunds(
                Exchange.Backtesting,
                typeof(SimpleBandWagonAlgorithm)));

            Assert.Throws<ArgumentNullException>(() => allocationManager.QueueTrade(
                new TradeProposal(new Balance(c, 10, 10)),
                typeof(SimpleBandWagonAlgorithm),
                Exchange.Backtesting,
                () => new TradeExecution(Balance.Empty(c), Balance.Empty(c))));
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
                     { typeof(SimpleBandWagonAlgorithm), factor }
                  }
                }
            });

            Currency c = new Currency("ETH");
            var local = SettingsService.BacktestInitialPortfolio.GetAllocation(c);
            var amount = alloc.GetAvailableFunds(Exchange.Backtesting, typeof(SimpleBandWagonAlgorithm), c);
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
                        { typeof(SimpleBandWagonAlgorithm), factor }
                    }
                }
            }));
        }

        [Fact]
        public void QueueTradeHappyFlow()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            var alloc = MakeDefaultAllocation();

            var weak = alloc.GetWeakAllocationManager(algo, Exchange.Backtesting);

            // Get most valuable asset from backtesting settings.
            Balance balance = SortedSettingsBalances.First();

            var proposal = new TradeProposal(balance);
            bool result = weak.QueueTrade(proposal, () =>
            {
                Logger.LogInformation($"Trading all of the {proposal.From.Free}{proposal.From.Symbol}");
                var exec = new TradeExecution(
                    proposal.From,
                    new Balance(new Currency("BTC"), 1, 0));

                // Mock real world side effects by changing the 'remote' portfolio
                _comms.RemotePortfolio.UpdateAllocation(exec);
                return exec;
            });

            Assert.True(result, "Valid trade was declared invalid");
            Assert.Equal(0.0M, weak.GetAvailableFunds(balance.Symbol).Free);
            Assert.Equal(1.0M, weak.GetAvailableFunds(new Currency("BTC")).Free);
        }

        [Fact]
        public void QueueTradeInvalid()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            Balance balance = SortedSettingsBalances.First();
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            var proposal = new TradeProposal(new Balance(
                balance.Symbol,
                balance.Free + 1,
                balance.Locked));

            bool result = alloc.QueueTrade(proposal, () =>
            {
                Logger.LogCritical("Invalid trade is being executed");
                Assert.True(false, "Trade callback of invalid trade is being executed");
                return null;
            });

            Assert.False(result, "Invalid proposal was reported as executed");
        }

        [Fact]
        public void QueueTradeNull()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            Assert.Throws<ArgumentNullException>(() => alloc.QueueTrade(null, () => null));
        }

        [Fact]
        public void QueueTradeReportNull()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            Balance balance = SortedSettingsBalances.First();
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            var proposal = new TradeProposal(balance);

            bool result = alloc.QueueTrade(proposal, () => null);
            Assert.True(result, "Valid proposal was not executed");

            // Assert that the allocation was not mutated
            Assert.Equal(proposal.From.Free, alloc.GetAvailableFunds(proposal.From.Symbol).Free);
            Assert.Equal(proposal.From.Locked, alloc.GetAvailableFunds(proposal.From.Symbol).Locked);
        }

        [Fact]
        public void GetAllFundsHappyFlow()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            var alloc = MakeDefaultAllocation().GetWeakAllocationManager(algo, Exchange.Backtesting);
            var funds = alloc.GetAllFunds();
            Assert.NotNull(funds);
            Assert.Equal(SortedSettingsBalances.Count(), funds.AllBalances().Count());
        }

        [Fact]
        public void GetAllFundEmpty()
        {
            Type algo = typeof(SimpleBandWagonAlgorithm);
            var totalalloc = new AllocationManager(LoggerFactory, _fetcher);
            totalalloc.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
            {
                { Exchange.Backtesting, new Dictionary<Type, decimal>() }
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
                        { typeof(SimpleBandWagonAlgorithm), scale }
                    }
                }
            });

            return alloc;
        }
    }
}