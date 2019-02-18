using System;
using System.Linq;
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
            public override State<FilteredSMACrossoverConfiguration> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.Zero);
            }
        }

        // Check for the filter SMA to be positive and the crossover to happen.
        private class EntryState : EntryState<FilteredSMACrossoverConfiguration>
        {
            public override State<FilteredSMACrossoverConfiguration> OnMarketCondition(DataProvider data)
            {
                // Check whether the filter SMA is hit or not.
                bool filterSma = data.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize)
                                 > data.GetStandardMovingAverage(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.FilterSMA,
                                     AlgorithmConfiguration.CandleSize,
                                     5);

                bool filterAtr = data.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.ShortATR)
                                 > data.GetAverageTrueRange(
                                     AlgorithmConfiguration.TradingPairs.First(),
                                     AlgorithmConfiguration.LongATR);

                // Check for the crossover to happen.
                bool crossoverSma = data.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.ShortSMA,
                                        AlgorithmConfiguration.CandleSize)
                                    > data.GetStandardMovingAverage(
                                        AlgorithmConfiguration.TradingPairs.First(),
                                        AlgorithmConfiguration.LongSMA,
                                        AlgorithmConfiguration.CandleSize);
                if (filterSma && crossoverSma && filterAtr)
                {
                    return new BuyState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<FilteredSMACrossoverConfiguration>
        {
            public override State<FilteredSMACrossoverConfiguration> OnTimerElapsed()
            {
                return new InTradeState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                SetTimer(TimeSpan.Zero);
            }
        }

        private class InTradeState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate stoploss;

            public override State<FilteredSMACrossoverConfiguration> OnTimerElapsed()
            {
                return new TrailingState(stoploss);
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // Get the lowest low from the last y hours.
                int candleamount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.DonchianMin;
                decimal donchianMinPrice = data.GetCandles(
                    AlgorithmConfiguration.TradingPairs.First(),
                    candleamount).Min(x => x.Low);

                // Set first stop loss order at DCMin.
                stoploss = trading.PlaceFullStoplossSell(AlgorithmConfiguration.TradingPairs.First(), donchianMinPrice);
                SetTimer(TimeSpan.Zero);
            }
        }

        private class TrailingState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate oldstop;

            public TrailingState(OrderUpdate stoploss)
            {
                oldstop = stoploss;
            }

            public override State<FilteredSMACrossoverConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == oldstop.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            public override State<FilteredSMACrossoverConfiguration> OnMarketCondition(DataProvider data)
            {
                int candleamount = AlgorithmConfiguration.CandleSize * AlgorithmConfiguration.DonchianMin;
                bool trail = data.GetCandles(
                                 AlgorithmConfiguration.TradingPairs.First(),
                                 candleamount).Min(x => x.Low)
                             >
                                 oldstop.SetPrice;

                if (trail)
                {
                    return new CancelState(oldstop);
                }

                return new NothingState<FilteredSMACrossoverConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class CancelState : State<FilteredSMACrossoverConfiguration>
        {
            private OrderUpdate oldstop;

            public CancelState(OrderUpdate stoploss)
            {
                oldstop = stoploss;
            }

            public override State<FilteredSMACrossoverConfiguration> OnTimerElapsed()
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