using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_DipBuy_BConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first short dip algorithm.
    /// buys when the market has an unnecessary dip, and sell after recovery.
    /// </summary>
    internal class Self_DipBuy_B : BaseAlgorithm<Config>
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
                bool dip = data.GetCandles(FirstPair, 3).RateOfChange()
                                       >
                           AlgorithmConfiguration.DipPercent;

                bool atrDeviation = data.GetCandles(FirstPair, 6).AverageTrueRange()
                                    >
                                    data.GetCandles(FirstPair, 26).AverageTrueRange();

                if (dip && atrDeviation)
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
                double waitMinutes = AlgorithmConfiguration.RecoveryTime * (int)AlgorithmConfiguration.CandleWidth;
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
    /// The Self_DipBuy_B settings.
    /// </summary>
    internal class Self_DipBuy_BConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets how much something needs to fall to be considered a dip. given like : 2% = 0.02.
        /// </summary>
        [RangeDecimal("0.005", "0.4")]
        public decimal DipPercent { get; set; }

        /// <summary>
        /// Gets or sets recovery, determines how many candles the system should wait before selling.
        /// </summary>
        [RangeInt(1, 100)]
        public int RecoveryTime { get; set; }
    }
}

#pragma warning restore SA1402