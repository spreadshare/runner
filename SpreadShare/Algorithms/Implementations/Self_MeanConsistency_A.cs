using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_MeanConsistency_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Buys when 6 closes are consistently lower than SMA and ATR5 is greater than ATR25.
    /// </summary>
    internal class Self_MeanConsistency_A : BaseAlgorithm<Config>
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
                var candles = data.GetCandles(FirstPair, AlgorithmConfiguration.SmaCounter * 2);

                bool entry = true;

                for (int i = 0; i < AlgorithmConfiguration.SmaCounter + 1; i++)
                {
                    var smaCandles = candles.Skip(i).Take(AlgorithmConfiguration.SmaCounter);
                    entry &= smaCandles.Last().Close < smaCandles.StandardMovingAverage();
                }

                if (entry)
                {
                    var atr50 = data.GetCandles(FirstPair, 51).AverageTrueRange();
                    var atr5 = data.GetCandles(FirstPair, 6).AverageTrueRange();
                    if (atr5 > atr50)
                    {
                        return new BuyAndSellState();
                    }
                }

                return new NothingState<Config>();
            }
        }

        private class BuyAndSellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderBuy(FirstPair);
                for (var i = 0; i < AlgorithmConfiguration.CandleCount; i++)
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
    internal class Self_MeanConsistency_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the number of consecutive times the sma condition has to be hit.
        /// </summary>
        [RangeInt(3, 10)]
        public int SmaCounter { get; protected set; }

        /// <summary>
        /// Gets or sets the number of candles to wait before selling.
        /// </summary>
        [RangeInt(5, 100)]
        public int CandleCount { get; protected set; }
    }
}

#pragma warning restore SA1402