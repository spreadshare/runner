using System;
using System.Linq;
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
            return new EntryState();
        }

        internal class EntryState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Welcome to the SimpleBandWagon strategy!");
                SwitchState(new CheckPositionValidity());
            }
        }

        /// <summary>
        /// Checks if the winner is not already the majority share of the portfolio.
        /// </summary>
        internal class CheckPositionValidity : State
        {
            protected override void ValidateContext()
            {
                //Retrieve global settings
                Currency baseSymbol = SettingsService.SimpleBandWagon.baseCurrency;
                int checkTime = SettingsService.SimpleBandWagon.checkTime;

                //Try to get to top performer, if not try state again after 10 seconds
                var winnerQuery = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (!winnerQuery.Success) { 
                    Logger.LogError($"Could not get top performer!\n{winnerQuery}\ntrying again after 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new CheckPositionValidity());
                    SwitchState(new TryAfterWaitState());
                    return;
                }

                //Calculate and show the percentage of increase
                var winnerPair = winnerQuery.Data.Item1; 
                decimal deltaPercentage = winnerQuery.Data.Item2 * 100 - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%"); 


                //Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < SettingsService.SimpleBandWagon.minimalGrowthPercentage) {
                    Logger.LogInformation($"Growth is less than {SettingsService.SimpleBandWagon.minimalGrowthPercentage}%, disregard.");
                    SwitchState(new WaitHoldingState());
                    return;
                }             

                //Retrieve all the assets to determine if perhaps the desired asset is already a majority share, in which case we do nothing.
                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success) {
                    Logger.LogError($"Could not get portfolio!\n{assetsQuery}\ntrying again after 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new CheckPositionValidity());
                    SwitchState(new TryAfterWaitState());
                    return;
                }
                var assets = assetsQuery.Data.GetAllFreeBalances();

                //1. Map the assets values to their respective baseSymbol values
                //2. Order by this newgained value, making the last element the most valuable.
                var sorted = assets.ToArray().Select(x =>
                    {
                        CurrencyPair pair;
                        try {
                            pair = CurrencyPair.Parse($"{x.Symbol}{baseSymbol}");
                        } catch {
                            return new AssetValue(x.Symbol, 0);
                        }
                        var query = TradingService.GetCurrentPrice(pair);
                        //Use a value of zero for assets whose price retrievals fail.
                        return query.Success ? new AssetValue(x.Symbol, x.Value * query.Data) : new AssetValue(x.Symbol, 0);
                    }
                ).OrderBy(x => x.Value);
                Logger.LogInformation($"Most valuable asset in portfolio: {sorted.Last().Symbol}");

                //Construct the most valueble asset as a currency
                Currency majorityAsset = new Currency(sorted.Last().Symbol);

                //Verify if this asset was also the top performer (winner)
                if (majorityAsset == winnerPair.Left) {
                    Logger.LogInformation($"Already in the possesion of the winner: {winnerPair}");
                    SwitchState(new WaitHoldingState());
                } else {
                    SwitchState(new RevertToBaseState());
                }
            }
        }
        /// <summary>
        /// Trades in all relevant assets for the base currency.
        /// </summary>
        internal class RevertToBaseState : State
        {
            protected override void ValidateContext()
            {
                //Retrieve globals from the settings.
                Currency baseSymbol = SettingsService.SimpleBandWagon.baseCurrency;
                decimal valueMinimum = SettingsService.SimpleBandWagon.minimalRevertValue;

                //Retrieve the portfolio, using a fallback in case of failure.
                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success) {
                    Logger.LogInformation("Could not get portfolio, going idle for 10 seconds, then try again.");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new RevertToBaseState());
                    SwitchState(new TryAfterWaitState());
                    return;
                }

                //Iterate through all the assets
                var assets = assetsQuery.Data.GetAllFreeBalances();
                foreach(var asset in assets) {

                    //Skip the base currency itself (ETHETH e.d. makes no sense)
                    if (asset.Symbol == baseSymbol.ToString()) continue;

                    //Try to get a valid pair against the base assets
                    CurrencyPair pair;
                    try {
                        pair = CurrencyPair.Parse($"{asset.Symbol}{baseSymbol}");
                    } catch(Exception) {
                        Logger.LogWarning($"{asset.Symbol}{baseSymbol} could not be parsed, is this asset listed on the exhange?"); 
                        continue;
                    }

                    //Get the price of pair (thus in terms of baseCurrency)
                    var priceQuery = TradingService.GetCurrentPrice(pair);
                    //In case of failure, just skip
                    if (!priceQuery.Success) { Logger.LogWarning($"Could not get price estimate for {pair}"); continue; }
                    decimal price = priceQuery.Data;

                    //Check if the eth value of the asset exceeds the minimum to be consired relevant
                    decimal value = price * asset.Value;
                    if (value >= valueMinimum) {
                        Logger.LogInformation($"Reverting for {pair}");
                        var orderQuery = TradingService.PlaceFullMarketOrder(pair, OrderSide.Sell);
                        if (!orderQuery.Success) {
                            Logger.LogWarning($"Reverting for {pair} failed! Is this pair trading on the exchange?");
                        }
                    } else {
                        Logger.LogInformation($"{pair} value not relevant ({value}{baseSymbol})");
                    }
                }
                Logger.LogInformation("Reverting to base succeeded.");
                SwitchState(new BuyState());
            }
        }
        /// <summary>
        /// Obtain the top performing coin
        /// (This will execute a trade even if the coin is already the majority share,
        /// consider to run the CheckPositionValidityState first.)
        /// </summary>
        internal class BuyState : State
        {
            protected override void ValidateContext()
            {
                //Retrieve globals from the settings.
                int checkTime = SettingsService.SimpleBandWagon.checkTime;

                //Try to retrieve the top performer, using a tryAfterWait fallback in case of failure.
                Logger.LogInformation($"Looking for the top performer from the previous {checkTime} hours");
                var query = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (query.Success) {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                } else {
                    Logger.LogWarning($"Could not fetch top performer, {query}\nRetyring state after 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new BuyState());
                    SwitchState(new TryAfterWaitState());
                    return;
                }

                //Calculate and show the percentage of increase
                var winnerPair = query.Data.Item1; 
                decimal deltaPercentage = query.Data.Item2 * 100 - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%"); 


                //Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < SettingsService.SimpleBandWagon.minimalGrowthPercentage) {
                    Logger.LogInformation($"Growth is less than {SettingsService.SimpleBandWagon.minimalGrowthPercentage}%, disregard.");
                    SwitchState(new WaitHoldingState());
                    return;
                } 

                //Place an order for the selected winner and goin into holding (again using a tryAfterWait fallback option)
                var response = TradingService.PlaceFullMarketOrder(query.Data.Item1, OrderSide.Buy);
                if (response.Success) {
                    SwitchState(new WaitHoldingState());
                } else {
                    Logger.LogInformation("Order has failed, retrying state in 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new BuyState());
                    SwitchState(new TryAfterWaitState());
                    return;
                }
            }
        }

        /// <summary>
        /// What as many hours as the holdTime dictactes and then proceed to checking the position again.
        /// </summary>
        internal class WaitHoldingState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation($"Going to sleep for {SettingsService.SimpleBandWagon.holdTime} hours ({DateTime.UtcNow})");
                //1000 ms / s
                //3600 s / h
                SetTimer(1000*3600*SettingsService.SimpleBandWagon.holdTime);
            }

            public override ResponseObject OnTimer() 
            {
                Logger.LogInformation($"Waking up! ({DateTime.UtcNow})");
                //First step after holding is verifying the current position.
                SwitchState(new CheckPositionValidity());
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        /// <summary>
        /// Helper state that enables 'try again after wait' solutions
        /// when exceptions pop up.
        /// This state returns to the state under the "TimerCallback" key in the
        /// Context after waiting for an amount of time specified under the "TimerIdleTime" key
        /// </summary>
        internal class TryAfterWaitState : State
        {
            long idleTime;
            State callback;
            protected override void ValidateContext()
            {
                try {
                    idleTime = (long)Context.GetObject("TimerIdleTime");
                    callback = (State)Context.GetObject("TimerCallback");
                } catch (Exception e) {
                    Logger.LogError($"TimerCallbackState could not validate the context\n{e.Message}");
                    Logger.LogCritical("No rational options, restarting the strategy...");
                    SwitchState(new EntryState());
                    return;
                }

                SetTimer(idleTime);
            }

            public override ResponseObject OnTimer() {
                SwitchState(callback);
                return new ResponseObject(ResponseCodes.Success);
            }
        }
    }
}
