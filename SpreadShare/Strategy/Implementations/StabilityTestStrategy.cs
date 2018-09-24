using System;
using System.Threading;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy.Implementations
{
    class StabilityTestStrategy : BaseStrategy
    {
        public StabilityTestStrategy(ILoggerFactory loggerFactory, ITradingService tradingService, IUserService userService) 
            : base(loggerFactory, tradingService, userService)
        {
        }
        public override State GetInitialState()
        {
            return new StartBuyState();
        }

        internal class StartBuyState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Welcome to the great stability test, no profit just checking.");
                var assets = UserService.GetPortfolio();
                foreach(var asset in assets.GetAllFreeBalances()) {
                    Logger.LogInformation($"{asset.Symbol} - {asset.Value}");
                }

                decimal price = 0;
                price = TradingService.GetCurrentPrice("BNBETH");
                Logger.LogInformation($"BNB is worth {price}ETH");
                while(price == 0) {
                    price = TradingService.GetCurrentPrice("BNBETH");
                    Logger.LogInformation("Trying again in a few seconds");
                    Thread.Sleep(10000);
                }
                decimal amount = assets.GetFreeBalance("ETH") / price;
                amount = Math.Floor(amount*100)/100;

                Logger.LogInformation($"Placing a buy order for {amount} BNB");
                long orderId = TradingService.PlaceMarketOrder("BNB", OrderSide.Buy, amount);
                Context.SetObject("OrderId", orderId);
                SwitchState(new ConfirmBuyOrderPlacedState());
            }
        }

        internal class ConfirmBuyOrderPlacedState : State
        {
            long orderId;
            protected override void ValidateContext()
            {
                try {
                    orderId = (long)Context.GetObject("OrderId");
                }
                catch(Exception e) {
                    Logger.LogInformation($"Could not validate context: {e.Message}");
                    throw e;
                }
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                if (order.ExecutionType == ExecutionType.New && order.OrderId == orderId) {
                    Logger.LogInformation("Order Placement Confirmed!");
                    SwitchState(new ConfirmBuyOrderTradedState());
                }
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class ConfirmBuyOrderTradedState : State
        {
            long orderId;
            protected override void ValidateContext()
            {
                try {
                    orderId = (long)Context.GetObject("OrderId");
                } catch(Exception e) {
                    Logger.LogInformation($"Could not validate context: {e.Message}");
                }
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                if (order.ExecutionType == ExecutionType.Trade && order.OrderId == orderId) {
                    Logger.LogInformation("Order Trade Confirmed!");
                    SwitchState(new SleepBeforeSellState());
                }
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class SleepBeforeSellState : State
        {
            protected override void ValidateContext()
            {
                //Set a timer for four hours
                SetTimer(1000* 3600 * 4);
                Logger.LogInformation("Going to sleep for a while (4 hours)");
            }

            public override ResponseObject OnTimer()
            {
                Logger.LogInformation("Waking up again!");
                SwitchState(new StartSellState());
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class StartSellState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Welcome to the great stability test, no profit, just checking.");
                var assets = UserService.GetPortfolio();
                foreach(var asset in assets.GetAllFreeBalances()) {
                    Logger.LogInformation($"{asset.Symbol} - {asset.Value}");
                }

                decimal amount = assets.GetFreeBalance("BNB");
                amount = Math.Floor(amount*100)/100;

                Logger.LogInformation($"Placing a sell order for {amount} BNB");

                long orderId = TradingService.PlaceMarketOrder("BNB", OrderSide.Sell, amount);
                Context.SetObject("OrderId", orderId);
                SwitchState(new SellOrderPlacedState());
            }
        }

        internal class SellOrderPlacedState : State
        {
            long orderId;
            protected override void ValidateContext()
            {
                try {
                    orderId = (long)Context.GetObject("OrderId");
                } catch (Exception e) {
                    Logger.LogInformation($"Could not validate context: {e.Message}");
                    throw e;
                }
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order) {
                if (order.ExecutionType == ExecutionType.New && order.OrderId == orderId) {
                    Logger.LogInformation("Order Placement Confirmed");
                    SwitchState(new SellOrderTradedState());
                }
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class SellOrderTradedState : State
        {
            long orderId;

            protected override void ValidateContext()
            {
                try {
                    orderId = (long)Context.GetObject("OrderId");
                } catch(Exception e) {
                    Logger.LogInformation($"Could not validate context {e.Message}");
                    throw e;
                }
            }

            public override ResponseObject OnOrderUpdate(BinanceStreamOrderUpdate order)
            {
                if (order.ExecutionType == ExecutionType.Trade && order.OrderId == orderId) {
                    Logger.LogInformation("Trade Confirmed!");
                    SwitchState(new SleepBeforeBuyState());
                }
                return new ResponseObject(ResponseCodes.Success);
            }
        }

        internal class SleepBeforeBuyState : State
        {
            protected override void ValidateContext()
            {
                //Set timer for 4 hours
                SetTimer(1000 * 3600 * 4);
                Logger.LogInformation("Goin to sleep for while (4 hours)");
            }

            public override ResponseObject OnTimer()
            {
                SwitchState(new StartBuyState());
                return new ResponseObject(ResponseCodes.Success);
            }
        }
    }
}