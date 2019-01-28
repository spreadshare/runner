using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first pumpfollowing algorithm.
    /// buys when the market has an large pump, and sells into an expected future pump
    /// </summary>
    internal class SimplePumpFollow : BaseAlgorithm<SimplePumpFollowSettings>
    {
        /// <inheritdoc />
        protected override EntryState<SimplePumpFollowSettings> Initial => new WelcomeState();

         // Buy when the price rises more than X percent in Y minutes, and sell after Z% followthrough or after A hours
         private class WelcomeState : EntryState<SimplePumpFollowSettings>
         {
             public override State<SimplePumpFollowSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }
         }

         private class EntryState : EntryState<SimplePumpFollowSettings>
         {
             public override State<SimplePumpFollowSettings> OnMarketCondition(DataProvider data)
             {
                 bool longPerformance = data.GetPerformancePastHours(
                                            AlgorithmSettings.ActiveTradingPairs.First(),
                             8).Data > (1 + AlgorithmSettings.FirstCheck);
                 bool shortPerformance = data.GetPerformancePastHours(
                                            AlgorithmSettings.ActiveTradingPairs.First(),
                                            3) < (1 - AlgorithmSettings.SecondCheck);
                if (longPerformance && shortPerformance)
                {
                    return new BuyState();
                }

                return new NothingState<SimplePumpFollowSettings>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<SimplePumpFollowSettings>
        {
            private OrderUpdate _limitsell;
            private bool _stophit;

            public override State<SimplePumpFollowSettings> OnMarketCondition(DataProvider data)
            {
                if (_stophit)
                {
                    return new StopState(_limitsell);
                }

                return new NothingState<SimplePumpFollowSettings>();
            }

            public override State<SimplePumpFollowSettings> OnTimerElapsed()
            {
                return new StopState(_limitsell);
            }

            public override State<SimplePumpFollowSettings> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _limitsell.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<SimplePumpFollowSettings>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                var buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                _limitsell = trading.PlaceFullLimitOrderSell(
                    AlgorithmSettings.ActiveTradingPairs.First(),
                    buyorder.AverageFilledPrice * AlgorithmSettings.ProfitTake);
                SetTimer(TimeSpan.FromHours(AlgorithmSettings.StopTime));

                var currentPrice = data.GetCurrentPriceLastTrade(AlgorithmSettings.ActiveTradingPairs.First());
                var sellPrice = buyorder.AverageFilledPrice * AlgorithmSettings.StopPrice;

                _stophit = currentPrice < sellPrice;
            }
        }

        private class StopState : State<SimplePumpFollowSettings>
        {
            private OrderUpdate oldlimit;

            public StopState(OrderUpdate limitsell)
            {
                oldlimit = limitsell;
            }

            public override State<SimplePumpFollowSettings> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldlimit);
                trading.ExecuteFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First());
                SetTimer(TimeSpan.Zero);
            }
        }
    }

    /// <summary>
    /// The SimplePumpFollow settings.
    /// </summary>
    internal class SimplePumpFollowSettings : AlgorithmSettings
    {
        /// <summary>
        /// Gets or sets how much something needs to rise in a short time to be considered a pump
        /// </summary>
        public decimal FirstCheck { get; set; }

        /// <summary>
        /// Gets or sets The SecondCheck, how much something needs to rise to be considered a followthrough
        /// </summary>
        public decimal SecondCheck { get; set; }

        /// <summary>
        /// Gets or sets profittake, determines where our exit should be
        /// </summary>
        public decimal ProfitTake { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out no matter the PnL
        /// </summary>
        public int StopTime { get; set; }

        /// <summary>
        /// Gets or sets StopPrice, determines how low our stop should be placed
        /// </summary>
        public decimal StopPrice { get; set; }
    }
}

#pragma warning restore SA1402