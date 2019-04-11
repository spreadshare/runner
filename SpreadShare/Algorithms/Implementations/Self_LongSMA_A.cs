using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_LongSMA_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// A filtered, simple SMA crossover system.
    /// Enters when longterm trend seems good, and shortterm trends shows a breakout.
    /// </summary>
    internal class Self_LongSMA_A : BaseAlgorithm<Config>
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
                // fix the SMA calls when the candles branch is merged
                bool smallCross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                  >
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAM).StandardMovingAverage();

                bool largeCross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAM).StandardMovingAverage()
                                  >
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAL).StandardMovingAverage();

                bool atrDeviation = data.GetCandles(FirstPair, 6).RateOfChange()
                                    >
                                    0.01m;

                if (smallCross && largeCross && atrDeviation)
                {
                    return new BuyState();
                }

                return new NothingState<Config>();
            }
        }

         // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class BuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecutePartialMarketOrderBuy(FirstPair, 0.7M);

                return new InTradeState();
            }
        }

        // This state checks whether to enter a pyramid order, trail the current stoploss or return to entry after closing
        private class InTradeState : State<Config>
        {
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                bool xSmallcross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                   <
                                   data.GetCandles(FirstPair, AlgorithmConfiguration.SMAM).StandardMovingAverage();

                bool xAtrDeviation = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                    >
                                    data.GetCandles(FirstPair, 26).AverageTrueRange();

                if (xSmallcross && xAtrDeviation)
                {
                    return new SellState();
                }

                return new NothingState<Config>();
            }
        }

        // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class SellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                trading.ExecuteFullMarketOrderSell(FirstPair);

                return new EntryState();
            }
        }
    }

    /// <summary>
    /// The Self_LongSMA_A settings.
    /// </summary>
    internal class Self_LongSMA_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the smallest SMA in amount of candles.
        /// </summary>
        [RangeInt(2, 25)]
        public int SMAS { get; set; }

        /// <summary>
        /// Gets or sets the medium SMA in amount of candles.
        /// </summary>
        [RangeInt(5, 75)]
        public int SMAM { get; set; }

        /// <summary>
        /// Gets or sets the large SMA in amount of candles.
        /// </summary>
        [RangeInt(25, 250)]
        public int SMAL { get; set; }
    }
}

#pragma warning restore SA1402