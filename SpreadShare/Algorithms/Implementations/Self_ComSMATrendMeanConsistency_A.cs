using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_ComSMATrendMeanConsistency_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Combined algorithm, combining a mean reversion, and a trend following algorithm.
    /// </summary>
    internal class Self_ComSMATrendMeanConsistency_A : BaseAlgorithm<Config>
    {
        /// <inheritdoc />
        protected override EntryState<Config> Initial => new WelcomeState();

        private class WelcomeState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                return new CheckState();
            }
        }

        private class CheckState : EntryState<Config>
        {
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                var candles = data.GetCandles(FirstPair, AlgorithmConfiguration.MeanEntry * 2);

                bool meanCondition = true;

                for (int i = 0; i < AlgorithmConfiguration.MeanEntry + 1; i++)
                {
                    var meanEntry = candles.Skip(i).Take(AlgorithmConfiguration.MeanEntry);
                    meanCondition &= meanEntry.Last().Close < meanEntry.StandardMovingAverage();
                }

                if (meanCondition)
                {
                    var atr50 = data.GetCandles(FirstPair, 51).AverageTrueRange();
                    var atr5 = data.GetCandles(FirstPair, 6).AverageTrueRange();
                    if (atr5 > atr50)
                    {
                        return new MeanBuyAndSellState();
                    }
                }

                bool smaCross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                    >
                                    data.GetCandles(FirstPair, AlgorithmConfiguration.SMAM).StandardMovingAverage();

                bool smaFilter = data.GetCandles(FirstPair, 50).StandardMovingAverage()
                                     >
                                     data.GetCandles(FirstPair, 200).StandardMovingAverage();

                bool atrDeviation = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                     >
                                     data.GetCandles(FirstPair, 51).AverageTrueRange();

                if (smaCross && smaFilter && atrDeviation)
                {
                    return new TrendBuyAndSellState();
                }

                return new NothingState<Config>();
            }
        }

        private class MeanBuyAndSellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderBuy(FirstPair);
                for (var i = 0; i < AlgorithmConfiguration.MeanWait; i++)
                {
                    WaitForNextCandle();
                }

                trading.ExecuteFullMarketOrderSell(FirstPair);
                return new CheckState();
            }
        }

        private class TrendBuyAndSellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderBuy(FirstPair);
                for (var i = 0; i < AlgorithmConfiguration.TrendWait; i++)
                {
                    WaitForNextCandle();
                }

                trading.ExecuteFullMarketOrderSell(FirstPair);
                return new CheckState();
            }
        }
    }

    /// <summary>
    /// The Self_MeanConsistency_A settings.
    /// </summary>
    internal class Self_ComSMATrendMeanConsistency_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the number of consecutive times the sma condition has to be hit.
        /// </summary>
        [RangeInt(3, 10)]
        public int MeanEntry { get; protected set; }

        /// <summary>
        /// Gets or sets the medium SMA in amount of candles.
        /// </summary>
        [RangeInt(5, 75)]
        public int SMAS { get; set; }

        /// <summary>
        /// Gets or sets the medium SMA in amount of candles.
        /// </summary>
        [RangeInt(5, 75)]
        public int SMAM { get; set; }

        /// <summary>
        /// Gets or sets the number of candles to wait before selling.
        /// </summary>
        [RangeInt(5, 100)]
        public int MeanWait { get; protected set; }

        /// <summary>
        /// Gets or sets the number of candles to wait before selling.
        /// </summary>
        [RangeInt(5, 100)]
        public int TrendWait { get; protected set; }
    }
}

#pragma warning restore SA1402