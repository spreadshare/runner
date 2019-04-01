using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.Stubs;
using Xunit;
using Xunit.Abstractions;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.Tests.ExchangeServices.AllocationTests
{
    public class AllocationManagerTests : BaseTest
    {
        private const string ObscureCoin = "SNGLS";
        private readonly IPortfolioFetcherService _fetcher;

        public AllocationManagerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            _fetcher = new TestPortfolioFetcher();
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var unused = new AllocationManager(LoggerFactory, _fetcher, null);
        }

        [Fact]
        public void ValidateAllocationsSetRequired()
        {
            var allocationManager = new AllocationManager(LoggerFactory, _fetcher, null);

            Currency c = new Currency("ETH");

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAvailableFunds(c));

            Assert.Throws<ArgumentNullException>(() => allocationManager.GetAllFunds());

            Assert.Throws<ArgumentNullException>(() => allocationManager.QueueTrade(
                new TradeProposal(TradingPair.Parse("EOSETH"), new Balance(c, 10, 10)),
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

        [Fact]
        public void SetAllocationHappyFlow()
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher, null);
            var currency = new Currency("ETH");
            alloc.SetInitialConfiguration(new Portfolio(new Dictionary<Currency, Balance>()
            {
                { currency, new Balance(currency, 2, 0) },
            }));

            Assert.Equal(2, alloc.GetAvailableFunds(currency).Free);
            Assert.Equal(0, alloc.GetAvailableFunds(currency).Locked);
        }

        [Fact]
        public void SetAllocationNoFreeFunds()
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher, null);
            var currency = new Currency("ETH");
            var allMoney = _fetcher.GetPortfolio().Data.GetAllocation(currency).Free;
            Assert.Throws<AllocationUnavailableException>(() =>
                alloc.SetInitialConfiguration(new Portfolio(new Dictionary<Currency, Balance>()
                {
                    { currency, new Balance(currency, allMoney + 1, 0) },
                })));
        }

        [Fact]
        public void SetAllocationNoLockedFunds()
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher, null);
            var currency = new Currency("ETH");
            Assert.Throws<AllocationUnavailableException>(() =>
                alloc.SetInitialConfiguration(new Portfolio(new Dictionary<Currency, Balance>()
                {
                    { currency, new Balance(currency, 1, 2) },
                })));
        }

        [Fact]
        public void QueueTradeHappyFlow()
        {
            var alloc = MakeDefaultAllocation();

            // Get most valuable asset from backtesting settings.
            decimal total = alloc.GetAvailableFunds(new Currency("ETH")).Free;
            decimal quote = alloc.GetAvailableFunds(new Currency("EOS")).Free;
            Balance balance = new Balance(new Currency("ETH"), total, 0);

            var proposal = new TradeProposal(TradingPair.Parse("EOSETH"), balance);

            var result = alloc.QueueTrade(proposal, () =>
            {
                Logger.LogInformation($"Trading all of the {proposal.From.Free}{proposal.From.Symbol}");

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

                return order;
            });

            Assert.True(result.Success, "Valid trade was declared invalid");
            Assert.Equal(total - proposal.From.Free, alloc.GetAvailableFunds(balance.Symbol).Free);
            Assert.Equal(quote + proposal.From.Free, alloc.GetAvailableFunds(new Currency("EOS")).Free);
        }

        [Fact]
        public void QueueTradeInvalid()
        {
            var alloc = MakeDefaultAllocation();
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
            var alloc = MakeDefaultAllocation();
            Assert.Throws<ArgumentNullException>(() => alloc.QueueTrade(null, () => null));
        }

        [Fact]
        public void QueueTradeReportZero()
        {
            var alloc = MakeDefaultAllocation();
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
            var alloc = MakeDefaultAllocation();
            var funds = alloc.GetAllFunds();
            Assert.NotNull(funds);
        }

        [Fact]
        public void GetAllFundEmpty()
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher, null);
            alloc.SetInitialConfiguration(new Portfolio(new Dictionary<Currency, Balance>()));
            var funds = alloc.GetAllFunds();
            Assert.NotNull(funds);
            Assert.Empty(funds.AllBalances());
        }

        private AllocationManager MakeDefaultAllocation()
        {
            var alloc = new AllocationManager(LoggerFactory, _fetcher, null);
            alloc.SetInitialConfiguration(_fetcher.GetPortfolio().Data);

            // Free up at least 10 SNGLS (ObscureCoin)
            alloc.UpdateAllocation(
                new TradeExecution(
                    Balance.Empty(new Currency(ObscureCoin)),
                    new Balance(new Currency(ObscureCoin), 10, 0)));
            return alloc;
        }
    }
}