using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy.Implementations
{
    internal class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory, ITradingService tradingService, IUserService userService) 
            : base(loggerFactory, tradingService, userService)
        { 
        }

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
                string baseSymbol = "ETH";
                decimal valueMinimum = 0.01M;

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
                        if (!orderQuery.Success) {
                            Logger.LogInformation($"Reverting for {pair} failed: {orderQuery}");
                        }
                    } else {
                        Logger.LogInformation($"{pair} value not relevant");
                    }
                }
                Logger.LogInformation("Reverting to base succeeded.");
            }
        }

        internal class BuyState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Looking for the top performer");
                var query = TradingService.GetTopPerformance(2, DateTime.Now);
                if (query.Success) {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                } else {
                    Logger.LogWarning($"Could not fetch top performer, {query}");
                }

                var response = TradingService.PlaceFullMarketOrder(CurrencyPair.Parse("VETETH"), OrderSide.Buy);
                if (response.Success) {
                    SwitchState(new WaitState());
                } else {
                    Logger.LogInformation("Order has failed, stalling...");
                }
            }
        }

        internal class WaitState : State
        {
            protected override void ValidateContext()
            {
                SetTimer(10*1000);
            }

            public override ResponseObject OnTimer() 
            {
                Console.WriteLine("Waited long enough, it's getting hot here!");
                SwitchState(new SellState());
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class SellState : State
        {
            protected override void ValidateContext()
            {
                TradingService.PlaceFullMarketOrder(CurrencyPair.Parse("VETETH"), OrderSide.Sell);
                Logger.LogInformation("I'm out!");
            }
        }
    }
}
