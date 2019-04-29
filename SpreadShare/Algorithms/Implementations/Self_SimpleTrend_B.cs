using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Config = SpreadShare.Algorithms.Implementations.Self_SimpleTrend_BConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// A filtered, simple SMA crossover system.
    /// Enters when longterm trend seems good, and shortterm trends shows a breakout.
    /// </summary>
    internal class Self_SimpleTrend_B : BaseAlgorithm<Config>
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

         // This Class buys the asset.
        private class BuyState : State<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                // If the Filter and CrossoverSMA signal the trade, we buy at market.
                OrderUpdate buyOrder = trading.ExecuteFullMarketOrderBuy(FirstPair);

                return new SetFirstStopState(buyOrder);
            }
        }

        // This Class Sets the first stoploss at a Donchian channel level.
        private class SetFirstStopState : State<Config>
        {
            private readonly OrderUpdate _buyOrder;
            private OrderUpdate stopLoss;

            public SetFirstStopState(OrderUpdate buyOrder)
            {
                _buyOrder = buyOrder;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (stopLoss != null && order.OrderId == stopLoss.OrderId
                                      &&
                                      order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                decimal stopPrice = data.GetLowestLow(FirstPair, AlgorithmConfiguration.Loss);
                stopLoss = trading.PlaceFullStoplossSell(FirstPair, stopPrice);

                return new InTradeState(_buyOrder, stopLoss);
            }
        }

        // This state checks whether to sell, or trail the stoploss higher
        private class InTradeState : State<Config>
        {
            private readonly OrderUpdate _buyOrder;
            private readonly OrderUpdate _stopLoss;

            public InTradeState(OrderUpdate buyOrder, OrderUpdate stopLoss)
            {
                _buyOrder = buyOrder;
                _stopLoss = stopLoss;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stopLoss != null && order.OrderId == _stopLoss.OrderId
                                      &&
                                      order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            public override State<Config> OnMarketCondition(DataProvider data)
            {
                bool crossUnder = data.GetCandles(FirstPair, AlgorithmConfiguration.SMAS).StandardMovingAverage()
                                  <
                                  data.GetCandles(FirstPair, AlgorithmConfiguration.SMAL).StandardMovingAverage();

                bool profitHit = data.GetCandles(FirstPair, 1)[0].Close
                                 >
                                 (_buyOrder.AverageFilledPrice * AlgorithmConfiguration.Profit);

                // Sell the asset because the conditions are unfavorable, or the profit has been hit
                if (crossUnder || profitHit)
                {
                    return new SellState(_stopLoss);
                }

                bool trail = data.GetLowestLow(FirstPair, AlgorithmConfiguration.Loss)
                              >
                             _stopLoss.StopPrice * 1.005m;

                // Trail the stop loss higher
                if (trail)
                {
                    return new CancelStopState(_stopLoss, _buyOrder);
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                return new NothingState<Config>();
            }
        }

        // This Class cancels the stop loss, and sells the asset
        private class SellState : State<Config>
        {
            private readonly OrderUpdate _stopLoss;

            public SellState(OrderUpdate stopLoss)
            {
                _stopLoss = stopLoss;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stopLoss != null && order.OrderId == _stopLoss.OrderId
                                      &&
                                      order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stopLoss);
                trading.ExecuteFullMarketOrderSell(FirstPair);

                return new EntryState();
            }
        }

        // This class cancels the stoploss so it can be trailed higher
        private class CancelStopState : State<Config>
        {
            private OrderUpdate _buyOrder;
            private OrderUpdate _stoploss;

            public CancelStopState(OrderUpdate stoploss, OrderUpdate buyOrder)
            {
                _stoploss = stoploss;
                _buyOrder = buyOrder;
            }

            public override State<Config> OnOrderUpdate(OrderUpdate order)
            {
                if (_stoploss != null && order.OrderId == _stoploss.OrderId
                                     &&
                    order.Status == OrderUpdate.OrderStatus.Filled)
                {
                    return new EntryState();
                }

                return new NothingState<Config>();
            }

            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                trading.CancelOrder(_stoploss);
                decimal stopPrice = data.GetLowestLow(FirstPair, AlgorithmConfiguration.Loss);
                OrderUpdate newStop = trading.PlaceFullStoplossSell(FirstPair, stopPrice);

                return new InTradeState(_buyOrder, newStop);
            }
        }
    }

    /// <summary>
    /// The Self_SimpleTrend_B settings.
    /// </summary>
    internal class Self_SimpleTrend_BConfiguration : AlgorithmConfiguration
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
        /// Gets or sets the large SMA in amount of candles.
        /// </summary>
        [RangeDecimal("1.05", "1.30")]
        public decimal Profit { get; set; }

        /// <summary>
        /// Gets or sets the large SMA in amount of candles.
        /// </summary>
        [RangeInt(10, 50)]
        public int Loss { get; set; }
    }
}

#pragma warning restore SA1402