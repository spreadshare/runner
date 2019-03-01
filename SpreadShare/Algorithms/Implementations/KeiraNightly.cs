using System;
using System.Linq;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.KeiraNightlyConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Dummy algorithm used for nightly backtests.
    /// </summary>
    internal class KeiraNightly : BaseAlgorithm<Config>
    {
        /// <inheritdoc/>
        protected override EntryState<Config> Initial => new MarketBuyState();

        private class MarketBuyState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                var pair = AlgorithmConfiguration.TradingPairs.First();
                trading.ExecuteFullMarketOrderBuy(pair);
                return new LimitSellState();
            }
        }

        private class LimitSellState : State<Config>
        {
            private OrderUpdate _sell;

            public override State<Config> OnTimerElapsed()
                => new CancelState(_sell, new StoplossSellState());

            public override State<KeiraNightlyConfiguration> OnOrderUpdate(OrderUpdate order)
            {
                if (_sell != null && order.OrderId == _sell.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new CoolDownState();
                }

                return new NothingState<KeiraNightlyConfiguration>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                var pair = AlgorithmConfiguration.TradingPairs.First();
                var price = data.GetCurrentPriceTopBid(pair);
                _sell = trading.PlaceFullLimitOrderSell(pair, price * AlgorithmConfiguration.TakeProfit);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.WaitTime));
                return new NothingState<Config>();
            }
        }

        private class StoplossSellState : State<Config>
        {
            private OrderUpdate _stoploss;

            public override State<KeiraNightlyConfiguration> OnTimerElapsed()
                => new CancelState(_stoploss, new MarketSellState());

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId && order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new CoolDownState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                var pair = AlgorithmConfiguration.TradingPairs.First();
                var price = data.GetCurrentPriceTopBid(pair);
                _stoploss = trading.PlaceFullStoplossSell(pair, price * AlgorithmConfiguration.StopTrail);
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.WaitTime));
                return new NothingState<KeiraNightlyConfiguration>();
            }
        }

        private class MarketSellState : State<Config>
        {
            protected override State<KeiraNightlyConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                var pair = AlgorithmConfiguration.TradingPairs.First();
                trading.ExecuteFullMarketOrderSell(pair);
                return new CoolDownState();
            }
        }

        private class CoolDownState : State<Config>
        {
            public override State<Config> OnTimerElapsed()
            {
                return new MarketBuyState();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                SetTimer(TimeSpan.FromHours(AlgorithmConfiguration.CoolDown));
                return new NothingState<Config>();
            }
        }

        private class CancelState : State<Config>
        {
            private readonly OrderUpdate _toCancel;
            private readonly State<Config> _continueWith;

            public CancelState(OrderUpdate toCancel, State<Config> continueWith)
            {
                _toCancel = toCancel;
                _continueWith = continueWith;
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_toCancel);
                return _continueWith;
            }
        }
    }

    /// <summary>
    /// Keira Nighly's configuration.
    /// </summary>
    internal class KeiraNightlyConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum number of hours to wait before an order is cancelled.
        /// </summary>
        [RangeInt(1, 50)]
        public int WaitTime { get; set; }

        /// <summary>
        /// Gets or sets the number of hours to wait before going to the entry state again.
        /// </summary>
        [RangeInt(1, 50)]
        public int CoolDown { get; set; }

        /// <summary>
        /// Gets or sets the ratio at which to place the limit sell.
        /// </summary>
        [RangeDecimal("1.0", "1.1")]
        public decimal TakeProfit { get; set; }

        /// <summary>
        /// Gets or sets the ratio at which to place the stoploss sell.
        /// </summary>
        [RangeDecimal("0.9", "1.0")]
        public decimal StopTrail { get; set; }
    }
}

#pragma warning restore SA1402