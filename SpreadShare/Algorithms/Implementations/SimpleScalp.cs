using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.SimpleScalpConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first simple scalp algorithm.
    /// buys without entry conditions and immediately sets an exit sell, with a time-based stop loss.
    /// </summary>
    internal class SimpleScalp : BaseAlgorithm<Config>
    {
        /// <inheritdoc />
        protected override EntryState<Config> Initial => new WelcomeState();

        // Buy at market, set a limit sell immediately, and a 2 hour stop. if the stop is hit, sell at market, and wait
        private class WelcomeState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        private class EntryState : EntryState<Config>
        {
            private OrderUpdate _limitSell;

            public override State<Config> OnTimerElapsed()
            {
                return new StopState(_limitSell);
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (order.Status == OrderUpdate.OrderStatus.Filled && order.OrderId == _limitSell.OrderId)
                {
                    return new WaitState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                OrderUpdate buyOrder =
                    trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                Portfolio portfolio = trading.GetPortfolio();
                _limitSell = trading.PlaceLimitOrderSell(
                    AlgorithmConfiguration.TradingPairs.First(),
                    portfolio.GetAllocation(AlgorithmConfiguration.TradingPairs.First().Left).Free,
                    buyOrder.AverageFilledPrice * AlgorithmConfiguration.TakeProfit);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.StopTime));
                return new NothingState<Config>();
            }
        }

        // On a successful trade, wait WaitTime minutes long and then restart putting in orders
        private class WaitState : State<Config>
        {
            public override State<Config> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation($"Total btc {trading.GetPortfolio().GetAllocation(new Currency("BTC"))}");
                SetTimer(TimeSpan.FromMinutes(AlgorithmConfiguration.WaitTime));
                return new NothingState<Config>();
            }
        }

        private class StopState : State<Config>
        {
            private readonly OrderUpdate oldLimit;

            public StopState(OrderUpdate limitSell)
            {
                oldLimit = limitSell;
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldLimit);
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
        /// Gets or sets at what point you take profit.
        /// </summary>
        [RangeDecimal("-1", "-1")]
        public decimal TakeProfit { get; set; }

        /// <summary>
        /// Gets or sets the WaitTime, basically a cooldown after exiting a trade.
        /// </summary>
        [RangeInt(-1, -1)]
        public int WaitTime { get; set; }

        /// <summary>
        /// Gets or sets the StopTime, determines how long to wait until we get out and try again.
        /// </summary>
        [RangeInt(-1, -1)]
        public int StopTime { get; set; }
    }
}

#pragma warning restore SA1402