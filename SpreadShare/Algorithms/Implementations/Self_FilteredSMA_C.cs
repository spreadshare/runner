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
    /// A filtered, simple SMA crossover system.
    /// Enters when longterm trend seems good, and shortterm trends shows a breakout.
    /// </summary>
    internal class Self_FilteredSMA_C : BaseAlgorithm<Self_FilteredSMA_CConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<Self_FilteredSMA_CConfiguration> Initial => new WelcomeState();

        private class WelcomeState : EntryState<Self_FilteredSMA_CConfiguration>
        {
            protected override State<Self_FilteredSMA_CConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        // Check for the filter SMA to be positive and the crossover to happen.
        private class EntryState : EntryState<Self_FilteredSMA_CConfiguration>
        {
            public override State<Self_FilteredSMA_CConfiguration> OnMarketCondition(DataProvider data)
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

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }
        }

         // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class BuyState : State<Self_FilteredSMA_CConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public BuyState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            protected override State<Self_FilteredSMA_CConfiguration> Run(TradingProvider trading, DataProvider data)
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
        private class CancelStopState : State<Self_FilteredSMA_CConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CancelStopState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Self_FilteredSMA_CConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }

            protected override State<Self_FilteredSMA_CConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stoploss);
                return new SetStopState(_pyramid);
            }
        }

        // This state sets a stoploss
        private class SetStopState : State<Self_FilteredSMA_CConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public SetStopState(int pyramid)
            {
                _pyramid = pyramid;
            }

            public override State<Self_FilteredSMA_CConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }

            protected override State<Self_FilteredSMA_CConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours.
                decimal donchianMinPrice = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin);

                // Set first stop loss order at DCMin.
                _stoploss = trading.PlaceFullStoplossSell(FirstPair, donchianMinPrice);
                return new CheckState(_stoploss, _pyramid);
            }
        }

         // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckState : State<Self_FilteredSMA_CConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Self_FilteredSMA_CConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }

            public override State<Self_FilteredSMA_CConfiguration> OnTimerElapsed()
            {
                return new CheckPyramidState(_stoploss, _pyramid);
            }

            public override State<Self_FilteredSMA_CConfiguration> OnMarketCondition(DataProvider data)
            {
                // Check whether we need to trail the stoploss higher
                bool trail = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin)
                             >
                             _stoploss.SetPrice;

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                SetTimer(TimeSpan.FromMinutes((int)AlgorithmConfiguration.CandleWidth));

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }
        }

        // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckPyramidState : State<Self_FilteredSMA_CConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckPyramidState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<Self_FilteredSMA_CConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }

            public override State<Self_FilteredSMA_CConfiguration> OnMarketCondition(DataProvider data)
            {
                // Check whether we need to trail the stoploss higher
                bool trail = data.GetLowestLow(FirstPair, AlgorithmConfiguration.DonchianMin)
                             >
                             _stoploss.SetPrice;

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

                return new NothingState<Self_FilteredSMA_CConfiguration>();
            }
        }
    }

    /// <summary>
    /// The Self_FilteredSMA_C settings.
    /// </summary>
    internal class Self_FilteredSMA_CConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the Shortterm crossover SMA in amount of candles.
        /// </summary>
        public int ShortSMA { get; set; }

        /// <summary>
        /// Gets or sets the longterm crossover SMA in amount of candles.
        /// </summary>
        public int LongSMA { get; set; }

        /// <summary>
        /// Gets or sets the Shortterm ATR for the filter in amount of candles.
        /// </summary>
        public int ShortATR { get; set; }

        /// <summary>
        /// Gets or sets the Longterm ATR for the filter in amount of candles.
        /// </summary>
        public int LongATR { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in amount of candles.
        /// </summary>
        public int DonchianMin { get; set; }
    }
}

#pragma warning restore SA1402