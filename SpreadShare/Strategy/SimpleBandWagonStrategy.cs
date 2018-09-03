using Microsoft.Extensions.Logging;
using SpreadShare.Services;
using System;

namespace SpreadShare.Strategy
{
    class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory, ITradingService tradingService) : base(loggerFactory, tradingService)
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
                Logger.LogInformation("Opening the entry state...");
                Logger.LogInformation("Placing buy order...");
                try
                {
                    long orderId = _stateManager.TradingService.PlaceMarketOrder("ETHBNB", Binance.Net.Objects.OrderSide.Buy);
                    Context.SetObject("orderId", orderId);
                }
                catch(Exception e)
                {
                    Logger.LogCritical("Buy order failed, exiting.");
                    Logger.LogCritical(e.Message);
                    throw;
                }
                SwitchState(new ConfirmBuyState());
            }

            public override void OnSomeAction()
            {
                Logger.LogInformation("Some action");
            }
        }

        internal class ConfirmBuyState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Validating context...");
                long orderId;
                try
                {
                    orderId = (long)Context.GetObject("orderId");
                }
                catch(Exception)
                {
                    Logger.LogCritical("Failed to validate context!");
                    throw;
                }
            }

            public override void OnSomeAction()
            {
                Logger.LogInformation("Some action");
                SwitchState(new EntryState());
            }
        }
    }
}
