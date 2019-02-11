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
    internal class ShortDip : BaseAlgorithm<ShortDipConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<ShortDipConfiguration> Initial => new WelcomeState();

        // Buy when the price dips more than X percent in Y minutes, and sell after Z% recovery or after A hours
        private class WelcomeState : EntryState<ShortDipConfiguration>
        {
            public override State<ShortDipConfiguration> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.Zero);
            }
        }

        private class EntryState : EntryState<ShortDipConfiguration>
        {
            public override State<ShortDipConfiguration> OnMarketCondition(DataProvider data)
            {
                bool performance = data.GetPerformancePastHours(
                                       AlgorithmConfiguration.TradingPairs.First(),
                                       AlgorithmConfiguration.DipTime) < (1 - AlgorithmConfiguration.DipPercent);
                if (performance)
                {
                    return new BuyState();
                }

                return new NothingState<ShortDipConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<ShortDipConfiguration>
        {
            private OrderUpdate _limitsell;

            public override State<ShortDipConfiguration> OnTimerElapsed()
            {
                return new CancelState(_limitsell);
            }

            public override State<ShortDipConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (order.OrderId == _limitsell.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<ShortDipConfiguration>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                var buyorder = trading.ExecuteFullMarketOrderBuy(AlgorithmConfiguration.TradingPairs.First());
                _limitsell = trading.PlaceFullLimitOrderSell(
                    AlgorithmConfiguration.TradingPairs.First(),
                    buyorder.AverageFilledPrice * AlgorithmConfiguration.Recovery);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.StopTime));
            }
        }

        private class CancelState : State<ShortDipConfiguration>
        {
            private OrderUpdate oldlimit;

            public CancelState(OrderUpdate limitsell)
            {
                oldlimit = limitsell;
            }

            public override State<ShortDipConfiguration> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(oldlimit);
                trading.ExecuteFullMarketOrderSell(AlgorithmConfiguration.TradingPairs.First());
                SetTimer(TimeSpan.Zero);
            }
        }
    }

    /// <summary>
    /// The ShortDip settings.
    /// </summary>
    internal class ShortDipConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets how much something needs to fall to be considered a dip.
        /// </summary>
        public decimal DipPercent { get; set; }

        /// <summary>
        /// Gets or sets The diptime, how quickly the dip needs to happen to be considered a dip.
        /// </summary>
        public double DipTime { get; set; }

        /// <summary>
        /// Gets or sets recovery, determines how much profit the system should try to get before selling.
        /// </summary>
        public decimal Recovery { get; set; }

        /// <summary>
        /// Gets or sets Stoptime, determines how long to wait untill we get out and try again.
        /// </summary>
        public int StopTime { get; set; }
    }
}

#pragma warning restore SA1402