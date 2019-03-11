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
    internal class FilteredSMACrossover : BaseAlgorithm<FilteredSMACrossoverConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<FilteredSMACrossoverConfiguration> Initial => new WelcomeState();

        private class WelcomeState : EntryState<FilteredSMACrossoverConfiguration>
        {
            protected override State<FilteredSMACrossoverConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        // Check for the filter SMA to be positive and the crossover to happen.
        private class EntryState : EntryState<FilteredSMACrossoverConfiguration>
        {
            public override State<FilteredSMACrossoverConfiguration> OnMarketCondition(DataProvider data)
            {
                // Check whether the filter SMA is hit or not.
                bool filterSma = DataProvider.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize)
                                 > DataProvider.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize,
                                     5);

                int shortAtrTime = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.ShortATR;
                int longAtrTime = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.LongATR;

                // Check whether the ATR is higher than average
                bool filterAtr = DataProvider.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     shortAtrTime,
                                     AlgorithmConfiguration.ShortATR)
                                 > DataProvider.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     longAtrTime,
                                     AlgorithmConfiguration.LongATR);

                // Check for the crossover to happen.
                bool crossoverSma = DataProvider.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.ShortSMA,
                                        AlgorithmConfiguration.CandleSize)
                                    > DataProvider.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.LongSMA,
                                        AlgorithmConfiguration.CandleSize);
                if (filterSma && crossoverSma && filterAtr)
                {
                    return new BuyState(null, 0);
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }
        }

         // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class BuyState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public BuyState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            protected override State<FilteredSMACrossoverConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                decimal allocation = trading.GetPortfolio().GetAllocation(
                                     AlgorithmConfiguration.BaseCurrency).Free
                                     *
                                     0.2M
                                     /
                                     data.GetCurrentPriceLastTrade(AlgorithmConfiguration.TradingPairs.First());

                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First(), allocation);
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
        private class CancelStopState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CancelStopState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<FilteredSMACrossoverConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            protected override State<FilteredSMACrossoverConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stoploss);
                return new SetStopState(_pyramid);
            }
        }

        // This state sets a stoploss
        private class SetStopState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public SetStopState(int pyramid)
            {
                _pyramid = pyramid;
            }

            public override State<FilteredSMACrossoverConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            protected override State<FilteredSMACrossoverConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours.
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.DonchianMin;
                decimal donchianMinPrice = data.GetCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    candleAmount).Min(x => x.Low);

                // Set first stop loss order at DCMin.
                _stoploss = trading.PlaceFullStoplossSell(AlgorithmConfiguration.TradingPairs.First(), donchianMinPrice);
                return new CheckState(_stoploss, _pyramid);
            }
        }

         // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<FilteredSMACrossoverConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            public override State<FilteredSMACrossoverConfiguration> OnTimerElapsed()
            {
                return new CheckPyramidState(_stoploss, _pyramid);
            }

            public override State<FilteredSMACrossoverConfiguration> OnMarketCondition(DataProvider data)
            {
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.DonchianMin;

                // Check whether we need to trail the stoploss higher
                var minPrice = data.GetCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    candleAmount).Min(x => x.Low);
                bool trail = minPrice > _stoploss.StopPrice;

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    Logger.LogInformation($"minPrice was {minPrice} which is more than stopPrice {_stoploss.StopPrice}");
                    return new CancelStopState(_stoploss, _pyramid);
                }

                SetTimer(TimeSpan.FromMinutes(AlgorithmConfiguration.CandleSize * 5));

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }
        }

        // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class CheckPyramidState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate _stoploss;
            private int _pyramid;

            public CheckPyramidState(OrderUpdate stoploss, int pyramid)
            {
                _stoploss = stoploss;
                _pyramid = pyramid;
            }

            public override State<FilteredSMACrossoverConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            public override State<FilteredSMACrossoverConfiguration> OnMarketCondition(DataProvider data)
            {
                int candleAmount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.DonchianMin;

                // Check whether we need to trail the stoploss higher
                bool trail = data.GetCandles(
                                 AlgorithmConfiguration.TradingPairs.First(),
                                 candleAmount).Min(x => x.Low)
                             >
                             _stoploss.SetPrice;

                // Check whether the filter SMA is hit or not.
                bool filterSma = DataProvider.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize)
                                 > DataProvider.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize,
                                     5);

                int shortAtrTime = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.ShortATR;
                int longAtrTime = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.LongATR;

                // Check whether the ATR is higher than average
                bool filterAtr = DataProvider.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     shortAtrTime,
                                     AlgorithmConfiguration.ShortATR)
                                 > DataProvider.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     longAtrTime,
                                     AlgorithmConfiguration.LongATR);

                // Check for the crossover to happen.
                bool crossoverSma = DataProvider.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.ShortSMA,
                                        AlgorithmConfiguration.CandleSize)
                                    > DataProvider.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.LongSMA,
                                        AlgorithmConfiguration.CandleSize);

                // If the entry requirements are hit, we pyramid into a bigger position
                if (filterSma && crossoverSma && filterAtr && _pyramid < 5)
                {
                    _pyramid++;
                    return new BuyState(_stoploss, _pyramid);
                }

                // If the trailing requirements are hit, we trail into a higher stoploss
                if (trail)
                {
                    return new CancelStopState(_stoploss, _pyramid);
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }
        }
    }

    /// <summary>
    /// The FilteredSMACrossover settings.
    /// </summary>
    internal class FilteredSMACrossoverConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the Filter SMA in amount of candles.
        /// </summary>
        public int FilterSMA { get; set; }

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
        /// Gets or sets the size of the candles for the SMA and Donchian in multiples of 5 minutes, 12 = 12*5 = 60 min.
        /// </summary>
        public int CandleSize { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in amount of candles.
        /// </summary>
        public int DonchianMin { get; set; }
    }
}

#pragma warning restore SA1402