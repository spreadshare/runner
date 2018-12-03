using System;
using System.Linq;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable

namespace SpreadShare.Algorithms.Implementations
{
    internal class SimpleScalp : BaseAlgorithm
     {
         /// <inheritdoc />
         public override ResponseObject Start(
             AlgorithmSettings settings,
             ExchangeProvidersContainer container,
             DatabaseContext database)
         {
             var stateManager = new StateManager<SimpleScalpSettings>(
                 settings as SimpleScalpSettings,
                 new WelcomeState(),
                 container,
                 database);

             return new ResponseObject(ResponseCode.Success);
         }

         // Buy at market, set a limit sell immediately, and a 2 hour stop. if the stop is hit, sell at market, and wait
         private class WelcomeState : State<SimpleScalpSettings>
         {
             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.Zero);
             }

             public override State<SimpleScalpSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }
         }
 
         private class EntryState : State<SimpleScalpSettings>
         {
             private OrderUpdate limitsell;
 
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

             public override State<SimpleScalpSettings> OnTimerElapsed()
             {
                 return new StopState(limitsell);
             }

             public override State<SimpleScalpSettings> OnOrderUpdate(OrderUpdate order)
             {
                 return new WaitState();
             }
         }
 
         // On a succesfull trade, wait WaitTime minutes long and then restart putting in orders
         private class WaitState : State<SimpleScalpSettings>
         {
             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 SetTimer(TimeSpan.FromMinutes(AlgorithmSettings.WaitTime));
             }

             public override State<SimpleScalpSettings> OnTimerElapsed()
             {
                 return new EntryState();
             }
         }
 
         private class StopState : State<SimpleScalpSettings>
         {
             private OrderUpdate oldlimit;
 
             public StopState(OrderUpdate limitsell)
             {
                 oldlimit = limitsell;
             }
             protected override void Run(TradingProvider trading, DataProvider data)
             {
                 trading.CancelOrder(oldlimit.Pair, oldlimit.OrderId);
                 OrderUpdate mktsell = trading.PlaceFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First())
                     .Data;
                 SetTimer(TimeSpan.Zero);
             }

             public override State<SimpleScalpSettings> OnTimerElapsed()
             {
                 return new WaitState();
             }
         }
     }

    internal class SimpleScalpSettings : AlgorithmSettings
    {
        public decimal TakeProfit { get; set; }
        public int WaitTime { get; set; }
        public int StopTime { get; set; }
    }
}

#pragma warning restore