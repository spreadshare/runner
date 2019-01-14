using System;
using System.Linq;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first short dip algorithm.
    /// buys when the market has an unesecary dip, and sell after a set amount of time
    /// </summary>
    internal class TimedShortDip : BaseAlgorithm<TimedShortDipSettings>
    {
        /// <inheritdoc />
        protected override EntryState<TimedShortDipSettings> Initial => new WelcomeState();

         // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
         private class WelcomeState : EntryState<TimedShortDipSettings>
         {
             public override State<TimedShortDipSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }
         }

         private class EntryState : EntryState<TimedShortDipSettings>
         {
             public override State<TimedShortDipSettings> OnMarketCondition(DataProvider data)
             {
                 bool performance = data.GetPerformancePastHours(
                                        AlgorithmSettings.ActiveTradingPairs.First(),
                             AlgorithmSettings.DipTime).Data < (1 - AlgorithmSettings.DipPercent);
                 if (performance)
                 {
                     return new BuyState();
                 }

                 return new NothingState<TimedShortDipSettings>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
             }
         }

         private class BuyState : State<TimedShortDipSettings>
         {
             private OrderUpdate buyorder;

             public override State<TimedShortDipSettings> OnTimerElapsed()
             {
                 return new ExitState(buyorder);
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 var buyorder = trading.PlaceFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                 SetTimer(TimeSpan.FromHours(AlgorithmSettings.ExitTime));
             }
         }

         private class ExitState : State<TimedShortDipSettings>
         {
             private OrderUpdate oldbuy;

             public ExitState(OrderUpdate buyorder)
             {
                 oldbuy = oldbuy;
             }

             public override State<TimedShortDipSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.PlaceFullMarketOrderSell(oldbuy.Pair);
                 SetTimer(TimeSpan.Zero);
             }
         }
     }

    /// <summary>
    /// The TimedShortDip settings
    /// </summary>
    internal class TimedShortDipSettings : AlgorithmSettings
    {
        /// <summary>
        /// Gets or sets how much something needs to fall to be considered a dip
        /// </summary>
        public decimal DipPercent { get; set; }

        /// <summary>
        /// Gets or sets The diptime, how quickly the dip needs to happen to be considered a dip
        /// </summary>
        public double DipTime { get; set; }

        /// <summary>
        /// Gets or sets recovery, determines how much profit the system should try to get before selling
        /// </summary>
        public decimal Recovery { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out and try again
        /// </summary>
        public int ExitTime { get; set; }
    }
}

#pragma warning restore SA1402