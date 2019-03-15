using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The Simple bandwagoning algorithm
    /// buys the highest performer of the last period.
    /// </summary>
    internal class Self_PumpMmtm_A : BaseAlgorithm<Self_PumpMmtm_AConfiguration>
    {
         /// <inheritdoc />
         protected override EntryState<Self_PumpMmtm_AConfiguration> Initial => new WelcomeState();

         // Buys the highest performer of the last number of hours
         private class WelcomeState : EntryState<Self_PumpMmtm_AConfiguration>
         {
             protected override State<Self_PumpMmtm_AConfiguration> Run(TradingProvider trading, DataProvider data)
             {
                 return new EntryState();
             }
         }

        // Checks for a winner among list of tradingpairs, once a winner is found, moves to buystate to enter.
         private class EntryState : EntryState<Self_PumpMmtm_AConfiguration>
         {
             public override State<Self_PumpMmtm_AConfiguration> OnMarketCondition(DataProvider data)
             {
                 (var winner, var performance) = data.GetTopPerformance(
                     AlgorithmConfiguration.TradingPairs,
                     AlgorithmConfiguration.CheckTime);

                 if (performance > (1 + AlgorithmConfiguration.Threshold))
                 {
                     return new BuyState(winner);
                 }

                 return new NothingState<Self_PumpMmtm_AConfiguration>();
             }
         }

        // Buys the highest performer, and moves into checkstate after HoldTime amount of hours
         private class BuyState : State<Self_PumpMmtm_AConfiguration>
         {
            private OrderUpdate _buyorder;
            private TradingPair _pair;

            public BuyState(TradingPair pair1)
            {
                 _pair = pair1;
            }

            public override State<Self_PumpMmtm_AConfiguration> OnTimerElapsed()
            {
                return new CheckState(_buyorder);
            }

            protected override State<Self_PumpMmtm_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                _buyorder = trading.ExecuteFullMarketOrderBuy(_pair);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.HoldTime));
                return new NothingState<Self_PumpMmtm_AConfiguration>();
            }
         }

        // Checks the performance of the trade after HoldTime.
        // If the current pair is still the best, it stays in the trade and waits an hour
        // If another pair is now the best, it switches to changestate
        // If no pairs are winners, it sells
         private class CheckState : State<Self_PumpMmtm_AConfiguration>
         {
            private OrderUpdate _oldbuy;

            public CheckState(OrderUpdate buyorder)
            {
                _oldbuy = buyorder;
            }

            public override State<Self_PumpMmtm_AConfiguration> OnMarketCondition(DataProvider data)
            {
                (var winner, var performance) = data.GetTopPerformance(
                    AlgorithmConfiguration.TradingPairs,
                    AlgorithmConfiguration.CheckTime);

                if (performance > (1 + AlgorithmConfiguration.Threshold)
                     &&
                     winner != _oldbuy.Pair)
                {
                     return new SellState(_oldbuy);
                }

                if (performance < (1 + AlgorithmConfiguration.Threshold))
                {
                     return new SellState(_oldbuy);
                }

                return new IdleState(_oldbuy);
            }
         }

        // If there are no winners, sell the current asset and moves back to scan for entries
         private class SellState : State<Self_PumpMmtm_AConfiguration>
         {
            private OrderUpdate _oldbuy;

            public SellState(OrderUpdate buyorder)
            {
                _oldbuy = buyorder;
            }

            protected override State<Self_PumpMmtm_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderSell(_oldbuy.Pair);
                return new EntryState();
            }
         }

        // If the current pair is still the best, wait an hour and go back to checkstate
         private class IdleState : State<Self_PumpMmtm_AConfiguration>
         {
            private OrderUpdate _oldbuy;

            public IdleState(OrderUpdate buyorder)
            {
                _oldbuy = buyorder;
            }

            public override State<Self_PumpMmtm_AConfiguration> OnTimerElapsed()
            {
                return new CheckState(_oldbuy);
            }

            protected override State<Self_PumpMmtm_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.FromHours(1));
                return new NothingState<Self_PumpMmtm_AConfiguration>();
            }
         }
    }

    /// <summary>
    /// The Self_PumpMmtm_A settings.
    /// </summary>
    internal class Self_PumpMmtm_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets how long back the algo checks in hours.
        /// </summary>
        public double CheckTime { get; set; }

        /// <summary>
        /// Gets or sets how long the algo needs to hold the best performer in hours.
        /// </summary>
        public double HoldTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum treshold a pair must have gained to pass the check in %, 10% is written as 0.10.
        /// </summary>
        public decimal Threshold { get; set; }
    }
}

#pragma warning restore SA1402