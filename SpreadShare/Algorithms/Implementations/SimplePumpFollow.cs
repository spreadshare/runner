using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first short dip algorithm.
    /// buys when the market has an unesecary dip, and sell after recovery.
    /// </summary>
    internal class SimplePumpFollow : BaseAlgorithm<SimplePumpFollowConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<SimplePumpFollowConfiguration> Initial => new WelcomeState();

        // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
        private class WelcomeState : EntryState<SimplePumpFollowConfiguration>
        {
            protected override State<SimplePumpFollowConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        private class EntryState : EntryState<SimplePumpFollowConfiguration>
        {
            public override State<SimplePumpFollowConfiguration> OnMarketCondition(DataProvider data)
            {
                bool longPerformance = data.GetPerformancePastHours(
                                           AlgorithmConfiguration.TradingPairs.First(),
                                           8) > (1 + AlgorithmConfiguration.FirstCheck);
                bool shortPerformance = data.GetPerformancePastHours(
                                            AlgorithmConfiguration.TradingPairs.First(),
                                            3) < (1 - AlgorithmConfiguration.SecondCheck);
                if (longPerformance && shortPerformance)
                {
                    return new BuyState();
                }

                return new NothingState<SimplePumpFollowConfiguration>();
            }
        }

        private class BuyState : State<SimplePumpFollowConfiguration>
        {
            private OrderUpdate _limitsell;
            private bool _stophit;

            public override State<SimplePumpFollowConfiguration> OnMarketCondition(DataProvider data)
            {
                if (_stophit)
                {
                    return new StopState(_limitsell);
                }

                return new NothingState<SimplePumpFollowConfiguration>();
            }

            public override State<SimplePumpFollowConfiguration> OnTimerElapsed()
            {
                return new StopState(_limitsell);
            }

            public override State<SimplePumpFollowConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _limitsell.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<SimplePumpFollowConfiguration>();
            }

            protected override State<SimplePumpFollowConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                var buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                _limitsell = trading.PlaceFullLimitOrderSell(
                    AlgorithmConfiguration.TradingPairs.First(),
                    buyorder.AverageFilledPrice * AlgorithmConfiguration.ProfitTake);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.StopTime));

                var currentPrice = data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First());
                var sellPrice = buyorder.AverageFilledPrice * AlgorithmConfiguration.StopPrice;

                _stophit = currentPrice < sellPrice;
                return new NothingState<SimplePumpFollowConfiguration>();
            }
        }

        private class StopState : State<SimplePumpFollowConfiguration>
        {
            private OrderUpdate oldlimit;

            public StopState(OrderUpdate limitsell)
            {
                oldlimit = limitsell;
            }

            protected override State<SimplePumpFollowConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldlimit);
                trading.ExecuteFullMarketOrderSell(AlgorithmConfiguration.TradingPairs.First());
                return new EntryState();
            }
        }
    }

    /// <summary>
    /// The SimplePumpFollow settings.
    /// </summary>
    internal class SimplePumpFollowConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets how much something needs to fall to be considered a dip.
        /// </summary>
        public decimal FirstCheck { get; set; }

        /// <summary>
        /// Gets or sets The diptime, how quickly the dip needs to happen to be considered a dip.
        /// </summary>
        public decimal SecondCheck { get; set; }

        /// <summary>
        /// Gets or sets recovery, determines how much profit the system should try to get before selling.
        /// </summary>
        public decimal ProfitTake { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out and try again.
        /// </summary>
        public int StopTime { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out and try again.
        /// </summary>
        public decimal StopPrice { get; set; }
    }
}

#pragma warning restore SA1402