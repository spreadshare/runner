using Microsoft.Extensions.Logging;
using System;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using Binance.Net.Objects;

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
                    long orderId = _stateManager.TradingService.PlaceMarketOrder("ETHBNB", OrderSide.Buy);
                    Context.SetObject("orderId", orderId);
                }
                catch(Exception e)
                {
                    Logger.LogCritical("Buy order failed, exiting.");
                    Logger.LogCritical(e.Message);
                    throw;
                }
                SwitchState(new ConfirmOrderPlacedState());
            }

            public override ResponseCodes OnCandle(Candle c)
            {
                Logger.LogInformation("Some action");
                return ResponseCodes.SUCCES;
            }

            public override ResponseCodes OnOrderUpdate(BinanceStreamOrderUpdate order) {
                return ResponseCodes.SUCCES;
            }
        }

        internal class ConfirmOrderPlacedState : State
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

            public override ResponseCodes OnOrderUpdate(BinanceStreamOrderUpdate order) {
                Logger.LogInformation($"Registered a new order with id: {order.OrderId}");
                if (order.OrderId == orderId && order.ExecutionType == ExecutionType.New) {
                    Logger.LogInformation($"Succesfully placed order!");
                    SwitchState(new ConfirmTradeState());
                }
                return ResponseCodes.SUCCES;
            }

            public override ResponseCodes OnTimer() {
                SwitchState(new WinnerState());
                return ResponseCodes.SUCCES;
            }
        }

        internal class ConfirmTradeState : State
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

            public override ResponseCodes OnOrderUpdate(BinanceStreamOrderUpdate order)
            {
                Logger.LogInformation($"Registered a new order with order id: {order.OrderId}");
                if (order.OrderId == orderId && order.ExecutionType == ExecutionType.Trade) {
                    Logger.LogInformation("Order Confirmed!");
                }
                return ResponseCodes.SUCCES;
            }
        }

        internal class WinnerState : State
        {
            protected override void ValidateContext()
            {
                //throw new NotImplementedException();
                Logger.LogInformation("YOU WIN!");
            }
        }
    }
}
