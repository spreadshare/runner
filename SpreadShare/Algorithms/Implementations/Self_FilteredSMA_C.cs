using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_FilteredSMA_CConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// A filtered, simple SMA crossover system.
    /// Enters when long term trend seems good, and short term trends shows a breakout.
    /// </summary>
    internal class Self_FilteredSMA_C : BaseAlgorithm<Config>
    {
        /// <inheritdoc />
        protected override EntryState<Config> Initial => new WelcomeState();

        private class WelcomeState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        // Check for the filter SMA to be positive and the crossover to happen.
        private class EntryState : EntryState<Config>
        {
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                // Check whether the filter SMA is hit or not.
                bool filterSma = data.GetCandles(FirstPair, 50).StandardMovingAverage()
                                 >
                                 data.GetCandles(FirstPair, 75).StandardMovingAverage();

                // Check whether the ATR is higher than average
                bool filterAtr = data.GetCandles(FirstPair, AlgorithmConfiguration.ShortATR).AverageTrueRange()
                                 >
                                 data.GetCandles(FirstPair, AlgorithmConfiguration.LongATR).AverageTrueRange();

                // Check for the crossover to happen.
                bool crossoverSma = data.GetCandles(FirstPair, AlgorithmConfiguration.ShortSMA).StandardMovingAverage()
                                    >
                                    data.GetCandles(FirstPair, AlgorithmConfiguration.LongSMA).StandardMovingAverage();
                if (filterSma && crossoverSma && filterAtr)
                {
                    return new BuyState(null, 0);
                }

                return new NothingState<Config>();
            }
        }

         // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class BuyState : State<Config>
        {
            private readonly OrderUpdate _stoploss;
            private readonly int _pyramid;

            public BuyState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                decimal allocation = trading.GetPortfolio().GetAllocation(
                                     AlgorithmConfiguration.BaseCurrency).Free
                                     *
                                     0.5M
                                     /
                                     data.GetCurrentPriceLastTrade(FirstPair);

                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteMarketOrderBuy(FirstPair, allocation);
                if (_stoploss != null)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }
                else
                {
                    return new SetStopState(_pyramid);
                }
            }
        }

        // This class cancels the current stop loss, and sets a new one.
        // At EVERY moment in a trade, this system should have a stoploss in place
        private class CancelStopState : State<Config>
        {
            private readonly OrderUpdate _stoploss;
            private readonly int _pyramid;

            public CancelStopState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stoploss);
                return new SetStopState(_pyramid);
            }
        }

        // This state sets a stoploss
        private class SetStopState : State<Config>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public SetStopState(int pyramid)
            {
                _pyramid = pyramid;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours.
                decimal donchianMinPrice = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin);

                // Set first stop loss order at DCMin.
                _stoploss = trading.PlaceFullStoplossSell(FirstPair, donchianMinPrice);
                return new CheckState(_stoploss, _pyramid);
            }
        }

         // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckState : State<Config>
        {
            private readonly OrderUpdate _stoploss;
            private readonly int _pyramid;

            public CheckState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            public override State<Config> OnTimerElapsed()
            {
                return new CheckPyramidState(_stoploss, _pyramid);
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                // Check whether we need to trail the stoploss higher
                bool trail = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin)
                             >
                             _stoploss.StopPrice;

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                SetTimer(TimeSpan.FromMinutes((int)AlgorithmConfiguration.CandleWidth));

                return new NothingState<Config>();
            }
        }

        // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckPyramidState : State<Config>
        {
            private readonly OrderUpdate _stoploss;
            private int _pyramid;

            public CheckPyramidState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                // Check whether we need to trail the stoploss higher
                bool trail = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin)
                             >
                             _stoploss.StopPrice;

                // Check whether the filter SMA is hit or not.
                bool filterSma = data.GetCandles(FirstPair, 50).StandardMovingAverage()
                                 >
                                 data.GetCandles(FirstPair, 75).StandardMovingAverage();

                // Check whether the ATR is higher than average
                bool filterAtr = data.GetCandles(FirstPair, AlgorithmConfiguration.ShortATR).AverageTrueRange()
                                 >
                                 data.GetCandles(FirstPair, AlgorithmConfiguration.LongATR).AverageTrueRange();

                // Check for the crossover to happen.
                bool crossoverSma = data.GetCandles(FirstPair, AlgorithmConfiguration.ShortSMA).StandardMovingAverage()
                                    >
                                    data.GetCandles(FirstPair, AlgorithmConfiguration.LongSMA).StandardMovingAverage();

                // If the entry requirements are hit, we pyramid into a bigger position
                if (filterSma && crossoverSma && filterAtr && _pyramid < 2)
                {
                    _pyramid++;
                    return new BuyState(_stoploss, _pyramid);
                }

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                return new NothingState<Config>();
            }
        }
    }

    /// <summary>
    /// The Self_FilteredSMA_C settings.
    /// </summary>
    internal class Self_FilteredSMA_CConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the Short term crossover SMA in amount of candles.
        /// </summary>
        [RangeInt(3, 15)]
        public int ShortSMA { get; set; }

        /// <summary>
        /// Gets or sets the long term crossover SMA in amount of candles.
        /// </summary>
        [RangeInt(5, 50)]
        public int LongSMA { get; set; }

        /// <summary>
        /// Gets or sets the Short term ATR for the filter in amount of candles.
        /// </summary>
        [RangeInt(3, 25)]
        public int ShortATR { get; set; }

        /// <summary>
        /// Gets or sets the Long term ATR for the filter in amount of candles.
        /// </summary>
        [RangeInt(5, 75)]
        public int LongATR { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in amount of candles.
        /// </summary>
        [RangeInt(10, 50)]
        public int DonchianMin { get; set; }
    }
}

#pragma warning restore SA1402