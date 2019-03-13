using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// The first short dip algorithm.
    /// buys when the market has an unesecary dip, and sell after recovery.
    /// </summary>
    internal class Self_DipBuy_B : BaseAlgorithm<Self_DipBuy_BConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<Self_DipBuy_BConfiguration> Initial => new WelcomeState();

        // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
        private class WelcomeState : EntryState<Self_DipBuy_BConfiguration>
        {
            protected override State<Self_DipBuy_BConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        private class EntryState : EntryState<Self_DipBuy_BConfiguration>
        {
            public override State<Self_DipBuy_BConfiguration> OnMarketCondition(DataProvider data)
            {
                // request the close of the current candle,
                // see if it is at least DipPercent below the close 2 candles ago
                bool dip = data.GetCandles(FirstPair, 1).close
                                       <
                           // request close 2 candles ago
                           (data.GetCandles(FirstPair, 1, 2).close * (1 - AlgorithmConfiguration.DipPercent));
                if (dip)
                {
                    return new BuyState();
                }

                return new NothingState<Self_DipBuy_BConfiguration>();
            }
        }

        private class BuyState : State<Self_DipBuy_BConfiguration>
        {
            protected override State<Self_DipBuy_BConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecutePartialMarketOrderBuy(FirstPair, 0.7M);
                return new WaitState();
            }
        }

        private class WaitState : State<Self_DipBuy_BConfiguration>
        {
            public override State<Self_DipBuy_BConfiguration> OnTimerElapsed()
            {
                return new SellState();
            }

            protected override State<Self_DipBuy_BConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                double WaitMinutes = AlgorithmConfiguration.RecoveryTime * (int) AlgorithmConfiguration.CandleWidth;
                SetTimer(TimeSpan.FromMinutes(WaitMinutes));
                return new NothingState<Self_DipBuy_BConfiguration>();
            }
        }
        
        private class SellState : State<Self_DipBuy_BConfiguration>
        {
            protected override State<Self_DipBuy_BConfiguration> Run(TradingProvider trading, DataProvider data)
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
        /// Gets or sets how much something needs to fall to be considered a dip.
        /// </summary>
        public decimal DipPercent { get; set; }

        /// <summary>
        /// Gets or sets recovery, determines how much profit the system should try to get before selling.
        /// </summary>
        public double RecoveryTime { get; set; }
    }
}

#pragma warning restore SA1402