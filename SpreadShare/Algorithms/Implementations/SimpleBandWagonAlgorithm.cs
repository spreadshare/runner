using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Simple bandwagon algorithm, works as follows.
    /// Starting Condition: 100% holding base currency
    /// Check most risen coin against base currency,
    /// if it performs better that a minimal percentage,
    /// fully change position to that asset and hold for the holdingTime before checking again.
    /// If their is no winner, remain in baseCurrency and check again after waitTime.
    /// </summary>
    internal class SimpleBandWagonAlgorithm : BaseAlgorithm
    {
        /// <inheritdoc />
        public override ResponseObject Start(
            AlgorithmSettings settings,
            ExchangeProvidersContainer container,
            DatabaseContext database)
        {
            var stateManager = new StateManager<SimpleBandWagonAlgorithmSettings>(
                settings as SimpleBandWagonAlgorithmSettings,
                new EntryState(),
                container,
                database);

            return new ResponseObject(ResponseCode.Success);
        }

        private class EntryState : State<SimpleBandWagonAlgorithmSettings>
        {
            public override State<SimpleBandWagonAlgorithmSettings> OnMarketCondition(DataProvider data)
            {
                decimal performance =
                    data.GetPerformancePastHours(AlgorithmSettings.ActiveTradingPairs.First(), AlgorithmSettings.CheckTime).Data;
                if (performance < 0.99M)
                {
                    Logger.LogInformation($"Panic detected buying");
                    return new BuyState();
                }

                return new NothingState<SimpleBandWagonAlgorithmSettings>();
            }
            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        private class BuyState : State<SimpleBandWagonAlgorithmSettings>
        {
            public override State<SimpleBandWagonAlgorithmSettings> OnTimerElapsed()
            {
                return new SellState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                decimal price = data.GetCurrentPriceLastTrade(AlgorithmSettings.ActiveTradingPairs.First()).Data;
                decimal quantity = 1M / price;
                trading.PlaceFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                SetTimer(TimeSpan.FromHours(AlgorithmSettings.HoldTime));
            }
        }

        private class SellState : State<SimpleBandWagonAlgorithmSettings>
        {
            public override State<SimpleBandWagonAlgorithmSettings> OnMarketCondition(DataProvider data)
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                trading.PlaceFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First());
            }
        }
    }
}