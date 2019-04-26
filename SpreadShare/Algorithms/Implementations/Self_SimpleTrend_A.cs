using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_SimpleTrend_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// A filtered, simple SMA crossover system.
    /// Enters when longterm trend seems good, and shortterm trends shows a breakout.
    /// </summary>
    internal class Self_SimpleTrend_A : BaseAlgorithm<Config>
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

                return new NothingState<Config>();
            }
        }

         // This Class buys the asset, and then either moves to set a new stop loss, or cancel the current one and reset
        private class BuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                OrderUpdate buyOrder = trading.ExecuteFullMarketOrderBuy(FirstPair);

                return new InTradeState(buyOrder);
            }
        }

        // This state checks whether to sell and return to entry after closing
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
                               (_buyOrder.AverageFilledPrice * AlgorithmConfiguration.Loss);

                if (crossUnder || profitHit || lossHit)
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
    /// The Self_SimpleTrend_A settings.
    /// </summary>
    internal class Self_SimpleTrend_AConfiguration : AlgorithmConfiguration
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
        /// Gets or sets the profit target for the trend.
        /// </summary>
        [RangeDecimal("1.05", "1.30")]
        public decimal Profit { get; set; }

        /// <summary>
        /// Gets or sets the max loss accepted for the algorithm.
        /// </summary>
        [RangeDecimal("0.80", "0.99")]
        public decimal Loss { get; set; }
    }
}

#pragma warning restore SA1402