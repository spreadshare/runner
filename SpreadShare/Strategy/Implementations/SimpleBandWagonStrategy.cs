using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy.Implementations
{
    class SimpleBandWagonStrategy : BaseStrategy
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
                }

                /* <-- Turned off so now unexpected trades take place -->
                var trade = TradingService.PlaceFullMarketOrder("ETHBNB", OrderSide.Sell);
                if (trade.Success) {
                    Logger.LogCritical("Trade succeeded!");
                } else {
                    Logger.LogCritical($"Trade failed: {trade}");
                } */
            }

            public override ResponseObject OnCandle(Candle c)
            {
                Logger.LogInformation("Some action");
                return new ResponseObject(ResponseCodes.Success);
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                return new ResponseObject(ResponseCodes.Success);
            }
        }
    }
}
