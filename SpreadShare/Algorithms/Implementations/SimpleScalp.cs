using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first simple scalp algorithm.
    /// buys without entryconditions and immediately sets an exit sell, with a time-based stop loss.
    /// </summary>
    internal class SimpleScalp : BaseAlgorithm<SimpleScalpConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<SimpleScalpConfiguration> Initial => new WelcomeState();

        // Buy at market, set a limit sell immediately, and a 2 hour stop. if the stop is hit, sell at market, and wait
        private class WelcomeState : EntryState<SimpleScalpConfiguration>
        {
            protected override State<SimpleScalpConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        private class EntryState : EntryState<SimpleScalpConfiguration>
        {
            private OrderUpdate _limitsell;

            public override State<SimpleScalpConfiguration> OnTimerElapsed()
            {
                return new StopState(_limitsell);
            }

            public override State<SimpleScalpConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.Status == OrderUpdate.OrderStatus.Filled && order.OrderId == _limitsell.OrderId)
                {
                    return new WaitState();
                }

                return new NothingState<SimpleScalpConfiguration>();
            }

            protected override State<SimpleScalpConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                OrderUpdate buyorder =
                    trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                Portfolio portfolio = trading.GetPortfolio();
                _limitsell = trading.PlaceLimitOrderSell(
                    AlgorithmConfiguration.TradingPairs.First(),
                    portfolio.GetAllocation(AlgorithmConfiguration.TradingPairs.First().Left).Free,
                    buyorder.AverageFilledPrice * AlgorithmConfiguration.TakeProfit);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.StopTime));
                return new NothingState<SimpleScalpConfiguration>();
            }
        }

        // On a successful trade, wait WaitTime minutes long and then restart putting in orders
        private class WaitState : State<SimpleScalpConfiguration>
        {
            public override State<SimpleScalpConfiguration> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override State<SimpleScalpConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation($"Total btc {trading.GetPortfolio().GetAllocation(new Currency("BTC"))}");
                SetTimer(TimeSpan.FromMinutes(AlgorithmConfiguration.WaitTime));
                return new NothingState<SimpleScalpConfiguration>();
            }
        }

        private class StopState : State<SimpleScalpConfiguration>
        {
            private OrderUpdate oldlimit;

            public StopState(OrderUpdate limitsell)
            {
                oldlimit = limitsell;
            }

            protected override State<SimpleScalpConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldlimit);
                trading.ExecuteFullMarketOrderSell(AlgorithmConfiguration.TradingPairs.First());
                return new WaitState();
            }
        }
    }

    /// <summary>
    /// The SimpleScalp settings.
    /// </summary>
    internal class SimpleScalpConfiguration : AlgorithmConfiguration
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