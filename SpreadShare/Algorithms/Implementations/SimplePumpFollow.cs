using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first pumpfollowing algorithm.
    /// buys when the market has an large pump, and sells into an expected future pump.
    /// </summary>
    internal class SimplePumpFollow : BaseAlgorithm<SimplePumpFollowConfiguration>
    {
         /// <inheritdoc />
         protected override EntryState<SimplePumpFollowConfiguration> Initial => new WelcomeState();

         // Buy when the price rises more than X percent in Y minutes, and sell after Z% followthrough or after A hours.
         private class WelcomeState : EntryState<SimplePumpFollowConfiguration>
         {
             public override State<SimplePumpFollowConfiguration> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }
         }

         private class EntryState : EntryState<SimplePumpFollowConfiguration>
         {
             public override State<SimplePumpFollowConfiguration> OnMarketCondition(DataProvider data)
             {
                bool longPerformance = data.GetPerformancePastHours(
                                           AlgorithmConfiguration.TradingPairs.First(),
                                           8) > (1 + AlgorithmConfiguration.LongCheck);
                bool shortPerformance = data.GetPerformancePastHours(
                                            AlgorithmConfiguration.TradingPairs.First(),
                                            3) < (1 - AlgorithmConfiguration.ShortCheck);
                if (longPerformance && shortPerformance)
                {
                    return new BuyState();
                }

                return new NothingState<SimplePumpFollowConfiguration>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
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

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                var buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                _limitsell = trading.PlaceFullLimitOrderSell(
                    AlgorithmConfiguration.TradingPairs.First(),
                    buyorder.AverageFilledPrice * AlgorithmConfiguration.ProfitTake);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.StopTime));

                var currentPrice = data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First());
                var sellPrice = buyorder.AverageFilledPrice * AlgorithmConfiguration.StopPrice;

                _stophit = currentPrice < sellPrice;
            }
         }

         private class StopState : State<SimplePumpFollowConfiguration>
         {
             private OrderUpdate oldlimit;

             public StopState(OrderUpdate limitsell)
             {
                 oldlimit = limitsell;
             }

             public override State<SimplePumpFollowConfiguration> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.CancelOrder(oldlimit);
                 trading.ExecuteFullMarketOrderSell(AlgorithmConfiguration.TradingPairs.First());
                 SetTimer(TimeSpan.Zero);
             }
         }
    }

    /// <summary>
    /// The SimplePumpFollow settings.
    /// </summary>
    internal class SimplePumpFollowConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets The longer check time to find the first pump. In hours.
        /// </summary>
        public decimal LongCheck { get; set; }

        /// <summary>
        /// Gets or sets The shorter check time, to indicate whether there is some followthrough. In hours.
        /// </summary>
        public decimal ShortCheck { get; set; }

        /// <summary>
        /// Gets or sets determines where our exit should be in percent. 5% is 1.05.
        /// </summary>
        public decimal ProfitTake { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out no matter the PnL. In Hours.
        /// </summary>
        public int StopTime { get; set; }

        /// <summary>
        /// Gets or sets StopPrice, determines how low our stop should be placed, 5% below is 0.95.
        /// </summary>
        public decimal StopPrice { get; set; }
    }
}

#pragma warning restore SA1402