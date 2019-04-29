using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_ComROCMeanSimpleTrend_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// A filtered, simple SMA crossover system.
    /// Enters when longterm trend seems good, and shortterm trends shows a breakout.
    /// </summary>
    internal class Self_ComROCMeanSimpleTrend_A : BaseAlgorithm<Config>
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

        // Check for the SMAs to crossover and the ATR to be high for the trend algo, and the ROC to hit for the ROC algo
        private class EntryState : EntryState<Config>
        {
            public override State<Config> OnMarketCondition(DataProvider data)
            {
                // fix the SMA calls when the candles branch is merged
                bool smallCross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                  >
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAM).StandardMovingAverage();

                bool largeCross = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                  >
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAL).StandardMovingAverage();

                bool filterAtr = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                 >
                                 data.GetCandles(FirstPair, 51).AverageTrueRange();

                if (smallCross && largeCross && filterAtr)
                {
                    return new BuyState();
                }

                // logic for the second part of the system, the ROCMean.
                bool filterRoc = data.GetCandles(FirstPair, AlgorithmConfiguration.ROCTime).RateOfChange()
                                 <
                                 -AlgorithmConfiguration.DipAmount;

                if (filterRoc && filterAtr)
                {
                    return new MeanBuyState();
                }

                return new NothingState<Config>();
            }
        }

        // Buy the asset for the mean algorithm
        private class MeanBuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderBuy(FirstPair);
                return new MeanSellState();
            }
        }

        // This state takes care of the sell order for the mean algorithm
        private class MeanSellState : State<Config>
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

         // This Class buys the asset.
        private class BuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                OrderUpdate buyOrder = trading.ExecuteFullMarketOrderBuy(FirstPair);

                return new InTradeState(buyOrder);
            }
        }

        // This state checks whether to close the trade
        private class InTradeState : State<Config>
        {
            private readonly OrderUpdate _buyOrder;

            public InTradeState(OrderUpdate buyOrder)
            {
                _buyOrder = buyOrder;
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                bool crossUnder = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                  <
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAL).StandardMovingAverage();

                bool profitHit = data.GetCandles(FirstPair, 1)[0].Close
                                 >
                                 (_buyOrder.AverageFilledPrice * AlgorithmConfiguration.Profit);

                bool lossHit = data.GetCandles(FirstPair, 1)[0].Close
                               <
                               (_buyOrder.AverageFilledPrice * 0.95m);

                if (crossUnder || profitHit || lossHit)
                {
                    return new SellState();
                }

                return new NothingState<Config>();
            }
        }

        // This Class sells the asset, and returns to the entrystate.
        private class SellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // Marketsell the asset and return to entry.
                trading.ExecuteFullMarketOrderSell(FirstPair);

                return new EntryState();
            }
        }
    }

    /// <summary>
    /// The Self_ComROCMeanSimpleTrend_A settings.
    /// </summary>
    internal class Self_ComROCMeanSimpleTrend_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the smallest SMA in amount of candles.
        /// </summary>
        [RangeInt(5, 30)]
        public int SMAS { get; set; }

        /// <summary>
        /// Gets or sets the medium SMA in amount of candles.
        /// </summary>
        [RangeInt(50, 100)]
        public int SMAM { get; set; }

        /// <summary>
        /// Gets or sets the large SMA in amount of candles.
        /// </summary>
        [RangeInt(75, 250)]
        public int SMAL { get; set; }

        /// <summary>
        /// Gets or sets the number of candles to hold after the mean entry.
        /// </summary>
        [RangeInt(2, 25)]
        public int ROCTime { get; protected set; }

        /// <summary>
        /// Gets or sets the entry requirement for the mean algoirthm.
        /// </summary>
        [RangeDecimal("0.005", "0.1")]
        public decimal DipAmount { get; protected set; }

        /// <summary>
        /// Gets or sets the profit factor for the trend algorithm.
        /// </summary>
        [RangeDecimal("1.05", "1.30")]
        public decimal Profit { get; set; }
    }
}

#pragma warning restore SA1402