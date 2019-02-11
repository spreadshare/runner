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
    /// buys when the market has an unnecessary dip, and sell after a set amount of time.
    /// </summary>
    internal class TimedShortDip : BaseAlgorithm<TimedShortDipConfiguration>
    {
         /// <inheritdoc />
         protected override EntryState<TimedShortDipConfiguration> Initial => new WelcomeState();

         // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours.
         private class WelcomeState : EntryState<TimedShortDipConfiguration>
         {
             public override State<TimedShortDipConfiguration> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }
         }

         private class EntryState : EntryState<TimedShortDipConfiguration>
         {
             public override State<TimedShortDipConfiguration> OnMarketCondition(DataProvider data)
             {
                 bool performance = data.GetPerformancePastHours(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.DipTime) < (1 - AlgorithmConfiguration.DipPercent);
                 if (performance)
                 {
                     return new BuyState();
                 }

                 return new NothingState<TimedShortDipConfiguration>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
             }
         }

         private class BuyState : State<TimedShortDipConfiguration>
         {
             private OrderUpdate _buyorder;

             public override State<TimedShortDipConfiguration> OnTimerElapsed()
             {
                 return new ExitState(_buyorder);
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 _buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                 SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.ExitTime));
             }
         }

         private class ExitState : State<TimedShortDipConfiguration>
         {
             private readonly OrderUpdate _oldbuy;

             public ExitState(OrderUpdate buyorder)
             {
                 _oldbuy = buyorder;
             }

             public override State<TimedShortDipConfiguration> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.ExecuteFullMarketOrderSell(_oldbuy.Pair);
                 SetTimer(TimeSpan.Zero);
             }
         }
     }

    /// <summary>
    /// The TimedShortDip settings.
    /// </summary>
    internal class TimedShortDipConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the percentage price needs to fall to start an entry, a 3% dip is written as 0.03.
        /// </summary>
        public decimal DipPercent { get; set; }

        /// <summary>
        /// Gets or sets The diptime, within how many hours the dip needs to happen.
        /// </summary>
        public double DipTime { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, after how many hours we consider the trade lost and marketsell out.
        /// </summary>
        public double ExitTime { get; set; }
    }
}

#pragma warning restore SA1402