using Microsoft.Extensions.Logging;
using System;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory, ITradingService tradingService, IUserService userService) 
            : base(loggerFactory, tradingService, userService)
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

            public override void OnCandle(Candle c)
            {
                Logger.LogInformation("Some action");
            }

            public override void OnNewOrder(Binance.Net.Objects.BinanceStreamOrderUpdate order) {

            }
        }

        internal class ConfirmBuyState : State
        {
            long orderId;
            protected override void ValidateContext()
            {
                Logger.LogInformation("Validating context...");
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

            public override void OnCandle(Candle c)
            {
                Logger.LogInformation("Some action");
               // SwitchState(new EntryState());
            }

            public override void OnNewOrder(Binance.Net.Objects.BinanceStreamOrderUpdate order) {
                Logger.LogInformation($"Registered a new order with id: {order.OrderId}");
                if (order.OrderId == orderId) {
                    Logger.LogInformation($"Succesfully placed order!");
                }
            }
        }
    }
}
