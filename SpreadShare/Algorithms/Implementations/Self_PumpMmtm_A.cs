using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_PumpMmtm_AConfiguration;

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
         protected override EntryState<Config> Initial => new WelcomeState();

         // Buys the highest performer of the last number of hours
         private class WelcomeState : EntryState<Config>
         {
             protected override State<Config> Run(TradingProvider trading, DataProvider data)
             {
                 return new EntryState();
             }
         }

        // Checks for a winner among list of tradingPairs, once a winner is found, moves to buystate to enter.
         private class EntryState : EntryState<Config>
         {
             public override State<Config> OnMarketCondition(DataProvider data)
             {
                 var (winner, performance) = data.GetTopPerformance(
                     AlgorithmConfiguration.TradingPairs,
                     AlgorithmConfiguration.CheckTime);

                 if (performance > (1 + AlgorithmConfiguration.Threshold))
                 {
                     return new BuyState(winner);
                 }

                 return new NothingState<Config>();
             }
         }

        // Buys the highest performer, and moves into checkstate after HoldTime amount of hours
         private class BuyState : State<Config>
         {
            private readonly TradingPair _pair;
            private OrderUpdate _buyOrder;

            public BuyState(TradingPair pair1)
            {
                 _pair = pair1;
            }

            public override State<Config> OnTimerElapsed()
            {
                return new CheckState(_buyOrder);
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                _buyOrder = trading.ExecuteFullMarketOrderBuy(_pair);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.HoldTime));
                return new NothingState<Config>();
            }
         }

        // Checks the performance of the trade after HoldTime.
        // If the current pair is still the best, it stays in the trade and waits an hour
        // If another pair is now the best, it switches to changestate
        // If no pairs are winners, it sells
         private class CheckState : State<Config>
         {
            private readonly OrderUpdate _oldBuy;

            public CheckState(OrderUpdate buyOrder)
            {
                _oldBuy = buyOrder;
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                var (winner, performance) = data.GetTopPerformance(
                    AlgorithmConfiguration.TradingPairs,
                    AlgorithmConfiguration.CheckTime);

                if (performance > (1 + AlgorithmConfiguration.Threshold)
                     &&
                     winner != _oldBuy.Pair)
                {
                     return new SellState(_oldBuy);
                }

                if (performance < (1 + AlgorithmConfiguration.Threshold))
                {
                     return new SellState(_oldBuy);
                }

                return new IdleState(_oldBuy);
            }
         }

        // If there are no winners, sell the current asset and moves back to scan for entries
         private class SellState : State<Config>
         {
            private readonly OrderUpdate _oldBuy;

            public SellState(OrderUpdate buyOrder)
            {
                _oldBuy = buyOrder;
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderSell(_oldBuy.Pair);
                return new EntryState();
            }
         }

        // If the current pair is still the best, wait an hour and go back to checkstate
         private class IdleState : State<Config>
         {
            private readonly OrderUpdate _oldBuy;

            public IdleState(OrderUpdate buyOrder)
            {
                _oldBuy = buyOrder;
            }

            public override State<Config> OnTimerElapsed()
            {
                return new CheckState(_oldBuy);
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.FromHours(1));
                return new NothingState<Config>();
            }
         }
    }

    /// <summary>
    /// The Self_PumpMmtm_A settings.
    /// </summary>
    internal class Self_PumpMmtm_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets how long back the algorithm checks in hours.
        /// </summary>
        [RangeInt(1, 120)]
        public int CheckTime { get; set; }

        /// <summary>
        /// Gets or sets how long the algorithm needs to hold the best performer in hours.
        /// </summary>
        [RangeInt(1, 120)]
        public int HoldTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum threshold a pair must have gained to pass the check in %, 10% is written as 0.10.
        /// </summary>
        [RangeDecimal("0.01", "0.1")]
        public decimal Threshold { get; set; }
    }
}

#pragma warning restore SA1402