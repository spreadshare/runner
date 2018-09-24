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
