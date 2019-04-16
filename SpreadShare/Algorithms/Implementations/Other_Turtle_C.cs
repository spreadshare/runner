using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Other_Turtle_CConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first Turtle inspired algorithm.
    /// Enters when long term trends are broken, sells when the opposite short term trend is broken.
    /// </summary>
    internal class Other_Turtle_C : BaseAlgorithm<Config>
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

        // Buy when the long term top gets broken.
        private class EntryState : EntryState<Config>
        {
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                bool priceFilter = data.GetCandles(FirstPair, 1)[0].High
                                   >= data.GetHighestHigh(FirstPair, AlgorithmConfiguration.LongTermTime);

                bool atrFilter = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                 > data.GetCandles(FirstPair, 51).AverageTrueRange();

                if (priceFilter && atrFilter)
                {
                    return new BuyState();
                }

                return new NothingState<Config>();
            }
        }

        private class BuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteFullMarketOrderBuy(FirstPair);

                for (var i = 0; i < AlgorithmConfiguration.WaitTime; i++)
                {
                    WaitForNextCandle();
                }

                return new SetStopState();
            }
        }

        // This state sets a stoploss
        private class SetStopState : State<Config>
        {
            private OrderUpdate _stoploss;

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
                decimal shortTermTimePrice = data.GetLowestLow(FirstPair, AlgorithmConfiguration.ShortTermTime);

                // Set first stop loss order at DCMin.
                _stoploss = trading.PlaceFullStoplossSell(FirstPair, shortTermTimePrice);
                return new CheckState(shortTermTimePrice, _stoploss);
            }
        }

        private class CheckState : State<Config>
        {
            private decimal stoplossPrice;
            private OrderUpdate stoploss;

            public CheckState(decimal stoplossPrice, OrderUpdate stoploss)
            {
                this.stoplossPrice = stoplossPrice;
                this.stoploss = stoploss;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (stoploss != null && order.OrderId == stoploss.OrderId
                                     &&
                    order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                decimal shortTermTimePrice = data.GetLowestLow(FirstPair, 10);

                if (shortTermTimePrice > stoplossPrice)
                {
                    return new CancelStopState(stoploss);
                }

                return new NothingState<Config>();
            }
        }

         // This class cancels the current stop loss, and sets a new one.
        // At EVERY moment in a trade, this system should have a stoploss in place
        private class CancelStopState : State<Config>
        {
            private readonly OrderUpdate _stoploss;

            public CancelStopState(OrderUpdate stoploss)
            {
                _stoploss = stoploss;
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
                return new SetStopState();
            }
        }
    }

    /// <summary>
    /// The Other_Turtle_C settings.
    /// </summary>
    internal class Other_Turtle_CConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the long term breakout line time in periods.
        /// </summary>
        [RangeInt(20, 250)]
        public int LongTermTime { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in periods.
        /// </summary>
        [RangeInt(5, 100)]
        public int ShortTermTime { get; set; }

        /// <summary>
        /// Gets or sets the short term breakout line time in periods.
        /// </summary>
        [RangeInt(5, 100)]
        public int WaitTime { get; set; }
    }
}

#pragma warning restore SA1402