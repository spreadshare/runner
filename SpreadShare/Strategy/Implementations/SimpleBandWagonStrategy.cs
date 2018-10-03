﻿using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy.Implementations
{
    internal class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory, ITradingService tradingService) 
            : base(loggerFactory, tradingService)
        {
        }

        public override State GetInitialState()
        {
            return new EntryState();
        }

        internal class EntryState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Welcome to the entry state, I do nothing yet.");
                var query = TradingService.GetTopPerformance(2, DateTime.Now);
                if (query.Success) {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                } else {
                    Logger.LogWarning($"Could not fetch top performer, {query}");
                }

                var response = TradingService.PlaceFullMarketOrder(CurrencyPair.Parse("BNBETH"), OrderSide.Sell);
                Logger.LogInformation(response.Success
                    ? "You Win!"
                    : $"What exactly do you think you are doing Hugo?\n{response}");
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                return new ResponseObject(ResponseCodes.Success);
            }
        }
    }
}