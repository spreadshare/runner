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
                    AlgorithmConfiguration.LongTermTime * AlgorithmConfiguration.CandleSize);

                // If the topLongTermPrice gets broken, we buy into the expected trend
                if (data.GetCandles(
                        AlgorithmConfiguration.TradingPairs.First(), 1).Max(x => x.High) >= topLongTermPrice)
                {
                    return new BuyState(null, 0);
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public BuyState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                if (_stoploss != null)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }
                else
                {
                    return new SetStopState(_pyramid);
                }
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                decimal allocation = trading.GetPortfolio().GetAllocation(
                                         AlgorithmConfiguration.BaseCurrency).Free
                                     *
                                     0.2M
                                     /
                                     data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First());

                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First(), allocation);
                SetTimer(TimeSpan.Zero);
            }
        }

         // This class cancels the current stop loss, and sets a new one.
        // At EVERY moment in a trade, this system should have a stoploss in place
        private class CancelStopState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CancelStopState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                return new SetStopState(_pyramid);
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stoploss);
                SetTimer(TimeSpan.Zero);
            }
        }

        // This state sets a stoploss
        private class SetStopState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public SetStopState(int pyramid)
            {
                _pyramid = pyramid;
            }

            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                return new CheckState(_stoploss, _pyramid);
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours.
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.ShortTermTime;
                decimal shortTermTimePrice = data.GetCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    candleAmount).Min(x => x.Low);

                // Set first stop loss order at DCMin.
                _stoploss = trading.PlaceFullStoplossSell(AlgorithmConfiguration.TradingPairs.First(), shortTermTimePrice);
                SetTimer(TimeSpan.Zero);
            }
        }

         // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<MidTermTurtleConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            public override State<MidTermTurtleConfiguration> OnTimerElapsed()
            {
                return new CheckPyramidState(_stoploss, _pyramid);
            }

            public override State<MidTermTurtleConfiguration> OnMarketCondition(DataProvider data)
            {
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.ShortTermTime;

                // Check whether we need to trail the stoploss higher
                bool trail = data.GetCandles(
                                 AlgorithmConfiguration.TradingPairs.First(),
                                 candleAmount).Min(x => x.Low)
                             >
                             _stoploss.SetPrice;

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                SetTimer(TimeSpan.FromMinutes(AlgorithmConfiguration.CandleSize * 5));

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckPyramidState : State<MidTermTurtleConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckPyramidState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

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
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.ShortTermTime;

                // Check whether we need to trail the stoploss higher
                bool trail = data.GetCandles(
                                 AlgorithmConfiguration.TradingPairs.First(),
                                 candleAmount).Min(x => x.Low)
                             >
                             _stoploss.SetPrice;

                // Get the highest high from the last X hours
                decimal topLongTermPrice = data.GetHighestHigh(
                    AlgorithmConfiguration.TradingPairs.First(),
                    AlgorithmConfiguration.LongTermTime * AlgorithmConfiguration.CandleSize);

                // If the topLongTermPrice gets broken, we buy into the expected trend
                if (data.GetCandles(
                        AlgorithmConfiguration.TradingPairs.First(), 1).Max(x => x.High) >= topLongTermPrice)
                {
                    return new BuyState(null, 0);
                }

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                return new NothingState<MidTermTurtleConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }
    }

    /// <summary>
    /// The MidTermTurtle settings.
    /// </summary>
    internal class MidTermTurtleConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the long term breakout line time in periods.
        /// </summary>
        public int LongTermTime { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in periods.
        /// </summary>
        public int ShortTermTime { get; set; }

        /// <summary>
        /// Gets or sets the size of the candles for the Donchian in multiples of 5 minutes, 12 = 12*5 = 60 min.
        /// </summary>
        public int CandleSize { get; set; }
    }
}

#pragma warning restore SA1402