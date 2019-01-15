using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first simple scalp algorithm.
    /// buys without entryconditions and immediately sets an exit sell, with a time-based stop loss.
    /// </summary>
    internal class SimpleScalp : BaseAlgorithm<SimpleScalpSettings>
    {
        /// <inheritdoc />
        protected override EntryState<SimpleScalpSettings> Initial => new WelcomeState();

        // Buy at market, set a limit sell immediately, and a 2 hour stop. if the stop is hit, sell at market, and wait
        private class WelcomeState : EntryState<SimpleScalpSettings>
        {
            public override State<SimpleScalpSettings> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.Zero);
            }
        }

        private class EntryState : EntryState<SimpleScalpSettings>
        {
            private OrderUpdate _limitsell;

            public override State<SimpleScalpSettings> OnTimerElapsed()
            {
                return new StopState(_limitsell);
            }

            public override State<SimpleScalpSettings> OnOrderUpdate(OrderUpdate order)
            {
                if (order.Status == OrderUpdate.OrderStatus.Filled && order.OrderId == _limitsell.OrderId)
                {
                    return new WaitState();
                }

                return new NothingState<SimpleScalpSettings>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                OrderUpdate buyorder =
                    trading.ExecuteFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                Portfolio portfolio = trading.GetPortfolio();
                _limitsell = trading.PlaceLimitOrderSell(
                    AlgorithmSettings.ActiveTradingPairs.First(),
                    portfolio.GetAllocation(AlgorithmSettings.ActiveTradingPairs.First().Left).Free,
                    buyorder.AverageFilledPrice * AlgorithmSettings.TakeProfit);
                SetTimer(TimeSpan.FromHours(AlgorithmSettings.StopTime));
            }
        }

        // On a succesfull trade, wait WaitTime minutes long and then restart putting in orders
        private class WaitState : State<SimpleScalpSettings>
        {
            public override State<SimpleScalpSettings> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.FromMinutes(AlgorithmSettings.WaitTime));
            }
        }

        private class StopState : State<SimpleScalpSettings>
        {
            private OrderUpdate oldlimit;

            public StopState(OrderUpdate limitsell)
            {
                oldlimit = limitsell;
            }

            public override State<SimpleScalpSettings> OnTimerElapsed()
            {
                return new WaitState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldlimit);
                trading.ExecuteFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First());
                SetTimer(TimeSpan.Zero);
            }
        }
    }

    /// <summary>
    /// The SimpleScalp settings.
    /// </summary>
    internal class SimpleScalpSettings : AlgorithmSettings
    {
        /// <summary>
        /// Gets or sets At what point you take profit.
        /// </summary>
        public decimal TakeProfit { get; set; }

        /// <summary>
        /// Gets or sets The waittime, basically a cooldown after exiting a trade.
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out and try again.
        /// </summary>
        public int StopTime { get; set; }
    }
}

#pragma warning restore SA1402