using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_ROCMean_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// An algorithm that buys when the ROC in a given timespan is sufficiently negative. quite similar to DipBuy_B.
    /// </summary>
    internal class Self_ROCMean_A : BaseAlgorithm<Config>
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
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                bool filterRoc = data.GetCandles(FirstPair, AlgorithmConfiguration.ROCTime).RateOfChange()
                                 <
                                 -AlgorithmConfiguration.DipAmount;
                bool filterSma = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                 >
                                 data.GetCandles(FirstPair, 51).AverageTrueRange();

                if (filterRoc && filterSma)
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
                trading.ExecuteFullMarketOrderBuy(FirstPair);
                return new SellState();
            }
        }

        private class SellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                for (int i = 0; i < 10; i++)
                {
                    WaitForNextCandle();
                }

                trading.ExecuteFullMarketOrderSell(FirstPair);
                return new EntryState();
            }
        }
    }

    /// <summary>
    /// The Self_ROCMean_A settings.
    /// </summary>
    internal class Self_ROCMean_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the number of consecutive times the sma condition has to be hit.
        /// </summary>
        [RangeInt(2, 10)]
        public int ROCTime { get; protected set; }

        /// <summary>
        /// Gets or sets the number of candles to wait before selling.
        /// </summary>
        [RangeDecimal("0.005", "0.1")]
        public decimal DipAmount { get; protected set; }
    }
}
#pragma warning restore SA1402