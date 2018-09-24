using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy.Implementations
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
                var orderIdQuery = TradingService.PlaceMarketOrder("ETHBNB", OrderSide.Buy, 1);
                if (orderIdQuery.Success)
                    Context.SetObject("orderId", orderIdQuery.Data);
                else
                    throw new Exception("Order placement failed!");
                SwitchState(new ConfirmOrderPlacedState());
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

        internal class ConfirmOrderPlacedState : State
        {
            long orderId;
            protected override void ValidateContext()
            {
                var assetsQuery = UserService.GetPortfolio();
                Assets assets = assetsQuery.Success ? assetsQuery.Data : throw new Exception("Could not get assets");
                var list = assets.GetAllLockedBalances();
                foreach(var item in list) {
                    Logger.LogInformation($"{item.Symbol} - {item.Value}");
                }
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

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                Logger.LogInformation($"Registered a new order with id: {order.OrderId}");
                if (order.OrderId == orderId && order.ExecutionType == ExecutionType.New) {
                    Logger.LogInformation($"Succesfully placed order!");
                    SwitchState(new ConfirmTradeState());
                }
                return new ResponseObject(ResponseCodes.Success);
            }

            public override ResponseObject OnTimer() {
                SwitchState(new WinnerState());
                return new ResponseObject(ResponseCodes.Success);
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

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order)
            {
                Logger.LogInformation($"Registered a new order with order id: {order.OrderId}");
                if (order.OrderId == orderId && order.ExecutionType == ExecutionType.Trade) {
                    Logger.LogInformation("Order Confirmed!");
                }
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class WinnerState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("YOU WIN!");
            }
        }
    }
}
