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
                decimal topLongTermPrice = data.GetHighestHigh(
                    AlgorithmConfiguration.TradingPairs.First(),
                    AlgorithmConfiguration.LongTermTime * 12);

                // If the topLongTermPrice gets broken, we buy into the expected trend
                if (data.GetCandles(
                        AlgorithmConfiguration.TradingPairs.First(), 1).Max(x => x.High) >= topLongTermPrice)
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
            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
                => new InTradeState();

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // If the long term top is broken, we buy at market, and move into the waiting state
                trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                SetTimer(TimeSpan.Zero);
            }
        }

        private class InTradeState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;

            public override State<MidTermTurtleConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            public override State<MidTermTurtleConfiguration> OnMarketCondition(DataProvider data)
            {
                bool trail = data.GetCandles(
                        AlgorithmConfiguration.TradingPairs.First(),
                        AlgorithmConfiguration.ShortTermTime * 12).Min(x => x.Low)
                             >
                             _stoploss.SetPrice;

                if (trail)
                {
                    return new CancelState(_stoploss);
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours
                decimal botShortTermPrice = data.GetCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    AlgorithmConfiguration.ShortTermTime * 12).Min(x => x.Low);
                _stoploss = trading.PlaceFullStoplossSell(AlgorithmConfiguration.TradingPairs.First(), botShortTermPrice);
            }
        }

        private class CancelState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate oldstop;

            public CancelState(OrderUpdate stoploss)
            {
                oldstop = stoploss;
            }

            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                return new InTradeState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldstop);
                SetTimer(TimeSpan.Zero);
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