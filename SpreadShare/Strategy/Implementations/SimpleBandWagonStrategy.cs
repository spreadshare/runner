﻿using System;
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
                Currency baseSymbol = SettingsService.SimpleBandWagon.baseCurrency;
                int checkTime = SettingsService.SimpleBandWagon.checkTime;

                var winnerQuery = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (!winnerQuery.Success) { 
                    Logger.LogError($"Could not get top performer!\n{winnerQuery}\ntrying again after 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new CheckPositionValidity());
                    SwitchState(new TryAfterWaitState());
                    return;
                }

                var winner = winnerQuery.Data.Item1; 
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winner} | {winnerQuery.Data.Item2 * 100}%");              

                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success) {
                    Logger.LogError($"Could not get portfolio!\n{assetsQuery}\ntrying again after 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new CheckPositionValidity());
                    SwitchState(new TryAfterWaitState());
                    return;
                }
                var assets = assetsQuery.Data.GetAllFreeBalances();
                var sorted = assets.ToArray().Select(x =>
                    {
                        CurrencyPair pair;
                        try {
                            pair = CurrencyPair.Parse($"{x.Symbol}{baseSymbol}");
                        } catch {
                            return new AssetValue(x.Symbol, 0);
                        }
                        var query = TradingService.GetCurrentPrice(pair);
                        if (query.Success) {
                            return new AssetValue(x.Symbol, x.Value * query.Data);
                        }
                        return new AssetValue(x.Symbol, 0);
                    }
                ).OrderBy(x => x.Value);
                Logger.LogInformation($"Most valuable asset in portfolio: {sorted.Last().Symbol}");

                if ($"{sorted.Last().Symbol}{baseSymbol}" == winner.ToString()) {
                    Logger.LogInformation($"Already in the possesion of the winner: {winner}");
                    SwitchState(new WaitState());
                } else {
                    SwitchState(new RevertToBaseState());
                }
            }
        }
        /// <summary>
        /// Trade in all relevant assets for the base currency.
        /// </summary>
        internal class RevertToBaseState : State
        {
            protected override void ValidateContext()
            {
                Currency baseSymbol = SettingsService.SimpleBandWagon.baseCurrency;
                decimal valueMinimum = SettingsService.SimpleBandWagon.minimalRevertValue;

                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success) {
                    Logger.LogInformation("Could not get portfolio, going idle for 10 seconds, then try again.");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new RevertToBaseState());
                    SwitchState(new TryAfterWaitState());
                    return;
                }
                var assets = assetsQuery.Data.GetAllFreeBalances();
                foreach(var asset in assets) {

                    //Skip the base currency itself (ETHETH e.d. makes no sense)
                    if (asset.Symbol == baseSymbol.ToString()) continue;

                    //Try to get a valid pair against the base assets
                    CurrencyPair pair;
                    try {
                        pair = CurrencyPair.Parse($"{asset.Symbol}{baseSymbol}");
                    } catch(Exception) {Logger.LogWarning($"{asset.Symbol}{baseSymbol} not listed on exchange"); continue;}

                    //Check if the value is relevant.
                    var priceQuery = TradingService.GetCurrentPrice(pair);
                    if (!priceQuery.Success) { Logger.LogWarning($"Could not get price estimate for {pair}"); continue; }
                    decimal price = priceQuery.Data;

                    //Check if the eth value of the asset exceeds the minimum to be consired relevant
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
        /// <summary>
        /// Obtain the top performing coin
        /// (This will execute a trade even if the coin is already the majority share,
        /// consider to run the CheckPositionValidityState first.)
        /// </summary>
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
                    Logger.LogInformation("Order has failed, retrying in 10 seconds");
                    Context.SetObject("TimerIdleTime", (long)10*1000);
                    Context.SetObject("TimerCallback", new BuyState());
                    SwitchState(new TryAfterWaitState());
                }
            }
        }

        /// <summary>
        /// What as many hours as the holdTime dictactes and then proceed to checking the position again.
        /// </summary>
        internal class WaitState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation($"Going to sleep for {SettingsService.SimpleBandWagon.holdTime} hours ({DateTime.Now.ToLocalTime()})");
                SetTimer(1000*3600*SettingsService.SimpleBandWagon.holdTime);
            }

            public override ResponseObject OnTimer() 
            {
                Logger.LogInformation($"Waking up! ({DateTime.Now.ToLocalTime()})");
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
                    throw e;
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
