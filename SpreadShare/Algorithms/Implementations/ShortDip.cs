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
    /// buys when the market has an unesecary dip, and sell after recovery
    /// </summary>
    internal class ShortDip : BaseAlgorithm<ShortDipSettings>
    {
        /// <inheritdoc />
        protected override EntryState<ShortDipSettings> Initial => new WelcomeState();

         // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
         private class WelcomeState : EntryState<ShortDipSettings>
         {
             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }
         }

         private class EntryState : EntryState<ShortDipSettings>
         {
             public override State<ShortDipSettings> OnMarketCondition(DataProvider data)
             {
                 bool performance = data.GetPerformancePastHours(
                                        AlgorithmSettings.ActiveTradingPairs.First(),
                             AlgorithmSettings.DipTime).Data < (1 - AlgorithmSettings.DipPercent);
                 if (performance)
                 {
                     return new BuyState();
                 }

                 return new NothingState<ShortDipSettings>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
             }
         }

         private class BuyState : State<ShortDipSettings>
         {
             private OrderUpdate limitsell;

             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new CancelState(limitsell);
             }

             public override State<ShortDipSettings> OnOrderUpdate(OrderUpdate order)
             {
                 if (order.OrderId == limitsell.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                 {
                     return new EntryState();
                 }

                 return new NothingState<ShortDipSettings>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 var buyorder = trading.PlaceFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                 limitsell = trading.PlaceFullLimitOrderSell(
                     AlgorithmSettings.ActiveTradingPairs.First(),
                     buyorder.Data.AverageFilledPrice * AlgorithmSettings.Recovery).Data;
                 SetTimer(TimeSpan.FromHours(AlgorithmSettings.StopTime));
             }
         }

         private class CancelState : State<ShortDipSettings>
         {
             private OrderUpdate oldlimit;

             public CancelState(OrderUpdate limitsell)
             {
                 oldlimit = limitsell;
             }

             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.CancelOrder(oldlimit.Pair, oldlimit.OrderId);
                 trading.PlaceFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First());
                 SetTimer(TimeSpan.Zero);
             }
         }
     }

    /// <summary>
    /// The ShortDip settings
    /// </summary>
    internal class ShortDipSettings : AlgorithmSettings
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
        public int StopTime { get; set; }
    }
}

#pragma warning restore SA1402