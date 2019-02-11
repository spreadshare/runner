using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first Turtle inspired algorithm.
    /// Enters when longterm trends are broken, sells when the opposite shortterm trend is broken.
    /// </summary>
    internal class MidTermTurtle : BaseAlgorithm<MidTermTurtleConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<MidTermTurtleConfiguration> Initial => new WelcomeState();

        private class WelcomeState : EntryState<MidTermTurtleConfiguration>
        {
            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.Zero);
            }
        }

        // Buy when the long term top gets broken.
        private class EntryState : EntryState<MidTermTurtleConfiguration>
        {
            public override State<MidTermTurtleConfiguration> OnMarketCondition(DataProvider data)
            {
                // Get the highest high from the last X hours
                decimal topLongTermPrice = data.GetFiveMinuteCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    AlgorithmConfiguration.LongTermTime * 12).Max(x => x.High);

                // If the topLongTermPrice gets broken, we buy into the expected trend
                if (data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First()) >= topLongTermPrice)
                {
                    return new BuyState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate buyorder;

            public override State<MidTermTurtleConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == buyorder.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new InTradeState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // If the long term top is broken, we buy at market, and move into the waiting state
                buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
            }
        }

        private class InTradeState : State<MidTermTurtleConfiguration>
        {
            public override State<MidTermTurtleConfiguration> OnMarketCondition(DataProvider data)
            {
                // Get the lowest low from the last y hours
                decimal botShortTermPrice = data.GetFiveMinuteCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    AlgorithmConfiguration.ShortTermTime * 12).Min(x => x.Low);

                // If the shortLongTermPrice gets broken, we sell into the expected trend change
                if (data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First()) <= botShortTermPrice)
                {
                    return new SellState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class SellState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate sellorder;

            public override State<MidTermTurtleConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == sellorder.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                sellorder = trading.ExecuteFullMarketOrderSell(AlgorithmConfiguration.TradingPairs.First());
            }
        }
    }

    /// <summary>
    /// The MidTermTurtle settings.
    /// </summary>
    internal class MidTermTurtleConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the long term breakout line time in hours.
        /// </summary>
        public int LongTermTime { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in hours.
        /// </summary>
        public int ShortTermTime { get; set; }
    }
}

#pragma warning restore SA1402