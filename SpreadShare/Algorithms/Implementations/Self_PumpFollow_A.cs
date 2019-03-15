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
    internal class Self_PumpFollow_A : BaseAlgorithm<Self_PumpFollow_AConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<Self_PumpFollow_AConfiguration> Initial => new WelcomeState();

        // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
        private class WelcomeState : EntryState<Self_PumpFollow_AConfiguration>
        {
            protected override State<Self_PumpFollow_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                return new EntryState();
            }
        }

        private class EntryState : EntryState<Self_PumpFollow_AConfiguration>
        {
            public override State<Self_PumpFollow_AConfiguration> OnMarketCondition(DataProvider data)
            {
                bool filterSma = data.GetCandles(FirstPair, 50).StandardMovingAverage()
                                 >
                                 data.GetCandles(FirstPair, 75).StandardMovingAverage();

                decimal crossvalue = (data.GetCandles(FirstPair, 5).AverageTrueRange()
                                      /
                                      data.GetCandles(FirstPair, 1).First().Close)
                                     * 2;

                bool pump = data.GetCandles(FirstPair, 3).RateOfChange() > crossvalue;

                if (filterSma && pump)
                {
                    return new BuyState();
                }

                return new NothingState<Self_PumpFollow_AConfiguration>();
            }
        }

        private class BuyState : State<Self_PumpFollow_AConfiguration>
        {
            protected override State<Self_PumpFollow_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                trading.ExecutePartialMarketOrderBuy(FirstPair, 0.7M);
                return new WaitState();
            }
        }

        private class WaitState : State<Self_PumpFollow_AConfiguration>
        {
            public override State<Self_PumpFollow_AConfiguration> OnTimerElapsed()
            {
                return new SellState();
            }

            protected override State<Self_PumpFollow_AConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                double waitMinutes = AlgorithmConfiguration.WaitTime * (int)AlgorithmConfiguration.CandleWidth;
                SetTimer(TimeSpan.FromMinutes(waitMinutes));
                return new NothingState<Self_PumpFollow_AConfiguration>();
            }
        }

        private class SellState : State<Self_PumpFollow_AConfiguration>
        {
            protected override State<Self_PumpFollow_AConfiguration> Run(TradingProvider trading, DataProvider data)
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
        /// Gets or sets Waittime, determines how long to wait untill we get out in candles.
        /// </summary>
        public double WaitTime { get; set; }
    }
}

#pragma warning restore SA1402