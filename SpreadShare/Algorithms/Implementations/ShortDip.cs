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
    internal class ShortDip : BaseAlgorithm
     {
         /// <inheritdoc />
         public override ResponseObject Start(
             AlgorithmSettings settings,
             ExchangeProvidersContainer container,
             DatabaseContext database)
         {
             var stateManager = new StateManager<ShortDipSettings>(
                 settings as ShortDipSettings,
                 new WelcomeState(),
                 container,
                 database);

             return new ResponseObject(ResponseCode.Success);
         }

         // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
         private class WelcomeState : State<ShortDipSettings>
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

         private class EntryState : State<ShortDipSettings>
         {
             private OrderUpdate limitsell;

             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new StopState(limitsell);
             }

             public override State<ShortDipSettings> OnOrderUpdate(OrderUpdate order)
             {
                 if (order.Status == OrderUpdate.OrderStatus.Filled && order.OrderId == limitsell.OrderId)
                 {
                     return new WaitState();
                 }

                 return new NothingState<ShortDipSettings>();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {


                 OrderUpdate buyorder = trading.PlaceFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First()).Data;
                 Portfolio portfolio = trading.GetPortfolio();
                 limitsell = trading.PlaceLimitOrderSell(
                    AlgorithmSettings.ActiveTradingPairs.First(),
                    portfolio.GetAllocation(AlgorithmSettings.ActiveTradingPairs.First().Left).Free,
                    buyorder.AverageFilledPrice * AlgorithmSettings.TakeProfit).Data;
                 SetTimer(TimeSpan.FromHours(AlgorithmSettings.StopTime));
             }
         }

         // On a succesfull trade, wait WaitTime minutes long and then restart putting in orders
         private class WaitState : State<ShortDipSettings>
         {
             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.FromMinutes(AlgorithmSettings.WaitTime));
             }
         }

         private class StopState : State<ShortDipSettings>
         {
             private OrderUpdate oldlimit;

             public StopState(OrderUpdate limitsell)
             {
                 oldlimit = limitsell;
             }

             public override State<ShortDipSettings> OnTimerElapsed()
             {
                 return new WaitState();
             }

             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.CancelOrder(oldlimit.Pair, oldlimit.OrderId);
                 OrderUpdate mktsell = trading.PlaceFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First())
                     .Data;
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
        public int DipTime { get; set; }

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