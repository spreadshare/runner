using System;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models
{
    public class TradeProposalTests : BaseTest
    {
        public TradeProposalTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ConstructorHappyFlow()
        {
            var pair = TradingPairTests.GetTradingPair("BNB", "ETH", 20);
            var proposal = new TradeProposal(pair, new Balance(new Currency("BNB"), 1, 0.00000001M));
            Assert.Equal(1, proposal.From.Free);
            Assert.Equal(0.00000001M, proposal.From.Locked);
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TradeProposal(null, new Balance(new Currency("BNB"), 0, 0)));
        }

        [Fact]
        public void ConstructorPerformsRounding()
        {
            var pair = TradingPairTests.GetTradingPair("BNB", "ETH", 2);
            var balance = new Balance(new Currency("BNB"), 42.1234569M, 0.000000000099M);
            var proposal = new TradeProposal(pair, balance);
            Assert.Equal(42.12M, proposal.From.Free);
            Assert.Equal(0, proposal.From.Locked);
        }

        [Fact]
        public void ConstructorNotMatchingCurrencies()
        {
            var pair = TradingPairTests.GetTradingPair("BNB", "ETH", 2);
            var balance = new Balance(new Currency("EOS"), 0, 0);
            Assert.Throws<ArgumentException>(() => new TradeProposal(pair, balance));
        }
    }
}