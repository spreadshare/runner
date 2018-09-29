using System;
using System.Threading;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy.Implementations
{
    internal class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory, ITradingService tradingService, 
           IUserService userService, ISettingsService settingsService) 
            : base(loggerFactory, tradingService, userService, settingsService)
        { }

        public override State GetInitialState()
        {
            return new RevertToBaseState();
        }

        internal class EntryState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Welcome to the SimpleBandWagon strategy!");
                SwitchState(new RevertToBaseState());
            }
        }

        internal class RevertToBaseState : State
        {
            protected override void ValidateContext()
            {
                Currency baseSymbol = SettingsService.SimpleBandWagon.baseCurrency;
                decimal valueMinimum = SettingsService.SimpleBandWagon.minimalRevertValue;

                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success) throw new Exception("Could not get portfolio!");
                var assets = assetsQuery.Data.GetAllFreeBalances();
                foreach(var asset in assets) {
                    //Try to get a valid pair against the base assets
                    CurrencyPair pair;
                    try {
                        pair = CurrencyPair.Parse($"{asset.Symbol}{baseSymbol}");
                    } catch(Exception) {Logger.LogInformation($"{asset.Symbol}{baseSymbol} not listed on exchange"); continue;}

                    //Check if the value is relevant.
                    var priceQuery = TradingService.GetCurrentPrice(pair);
                    if (!priceQuery.Success) { Logger.LogWarning($"Could not get price estimate for {pair}"); continue; }
                    decimal price = priceQuery.Data;

                    decimal value = price * asset.Value;
                    if (value >= valueMinimum) {
                        Logger.LogInformation($"Reverting for {pair}");
                        var orderQuery = TradingService.PlaceFullMarketOrder(pair, OrderSide.Sell);
                        while (!orderQuery.Success) {
                            Logger.LogInformation($"Reverting for {pair} failed: {orderQuery}");
                            Logger.LogInformation($"Retrying in a few seconds...");
                            Thread.Sleep(4000);
                            orderQuery = TradingService.PlaceFullMarketOrder(pair, OrderSide.Sell);
                        }
                    } else {
                        Logger.LogInformation($"{pair} value not relevant");
                    }
                }
                Logger.LogInformation("Reverting to base succeeded.");
                SwitchState(new BuyState());
            }
        }

        internal class BuyState : State
        {
            protected override void ValidateContext()
            {
                int checkTime = SettingsService.SimpleBandWagon.checkTime;
                Logger.LogInformation($"Looking for the top performer from the previous {checkTime} hours");
                var query = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (query.Success) {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                } else {
                    Logger.LogWarning($"Could not fetch top performer, {query}");
                }

                var response = TradingService.PlaceFullMarketOrder(query.Data.Item1, OrderSide.Buy);
                if (response.Success) {
                    SwitchState(new WaitState());
                } else {
                    Logger.LogInformation("Order has failed, retrying...");
                    SwitchState(new BuyState());
                }
            }
        }

        internal class WaitState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation($"Going to sleep for {SettingsService.SimpleBandWagon.holdTime} hours ({DateTime.Now.ToLocalTime().ToString()})");
                //SetTimer(1000*3600*SettingsService.SimpleBandWagon.holdTime);
                SetTimer(1000*5);
            }

            public override ResponseObject OnTimer() 
            {
                Logger.LogInformation("Waking up!");
                SwitchState(new RevertToBaseState());
                return new ResponseObject(ResponseCodes.Success);
            }
        }
    }
}
