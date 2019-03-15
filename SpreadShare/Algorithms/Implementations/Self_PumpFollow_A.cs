using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_PumpFollow_AConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first short dip algorithm.
    /// buys when the market has an unnecessary dip, and sell after recovery.
    /// </summary>
    internal class Self_PumpFollow_A : BaseAlgorithm<Config>
    {
        /// <inheritdoc />
        protected override EntryState<Config> Initial => new WelcomeState();

        // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
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
                bool filterSma = data.GetCandles(FirstPair, 50).StandardMovingAverage()
                                 >
                                 data.GetCandles(FirstPair, 75).StandardMovingAverage();

                decimal crossValue = (data.GetCandles(FirstPair, 5).AverageTrueRange()
                                      /
                                      data.GetCandles(FirstPair, 1).First().Close)
                                     * 2;

                bool pump = data.GetCandles(FirstPair, 3).RateOfChange() > crossValue;

                if (filterSma && pump)
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
                trading.ExecutePartialMarketOrderBuy(FirstPair, 0.7M);
                return new WaitState();
            }
        }

        private class WaitState : State<Config>
        {
            public override State<Config> OnTimerElapsed()
            {
                return new SellState();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                double waitMinutes = AlgorithmConfiguration.WaitTime * (int)AlgorithmConfiguration.CandleWidth;
                SetTimer(TimeSpan.FromMinutes(waitMinutes));
                return new NothingState<Config>();
            }
        }

        private class SellState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecuteFullMarketOrderSell(FirstPair);
                return new EntryState();
            }
        }
    }

    /// <summary>
    /// The Self_PumpFollow_A settings.
    /// </summary>
    internal class Self_PumpFollow_AConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets WaitTime, determines how long to wait until we get out in candles.
        /// </summary>
        [RangeInt(1, 50)]
        public int WaitTime { get; set; }
    }
}

#pragma warning restore SA1402