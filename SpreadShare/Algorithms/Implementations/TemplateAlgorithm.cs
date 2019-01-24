using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Stud algorithm, used for testing.
    /// </summary>
    internal class TemplateAlgorithm : BaseAlgorithm<TemplateAlgorithmSettings>
    {
        /// <inheritdoc />
        protected override EntryState<TemplateAlgorithmSettings> Initial => new WelcomeState();

        private class WelcomeState : EntryState<TemplateAlgorithmSettings>
        {
            public override State<TemplateAlgorithmSettings> OnTimerElapsed()
            {
                return new TemplateState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("Welcome to the TemplateAlgorithm");
                SetTimer(TimeSpan.Zero);
            }
        }

        private class TemplateState : EntryState<TemplateAlgorithmSettings>
        {
            protected override void Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("I wonder if Miss Bitcoin thinks I should buy...");
                decimal price = data.GetCurrentPriceTopAsk(AlgorithmSettings.ActiveTradingPairs.First());
                var order = trading.PlaceFullLimitOrderBuy(AlgorithmSettings.ActiveTradingPairs.First(), price * 0.95M);
                trading.CancelOrder(order);
                trading.ExecuteFullMarketOrderBuy(AlgorithmSettings.ActiveTradingPairs.First());
                order = trading.PlaceFullLimitOrderSell(AlgorithmSettings.ActiveTradingPairs.First(), price * 1.05M);
                trading.CancelOrder(order);
                trading.ExecuteFullMarketOrderSell(AlgorithmSettings.ActiveTradingPairs.First());
            }

            private string ShowAlloc(TradingProvider trading)
            {
                var alloc = trading.GetPortfolio();
                var left = AlgorithmSettings.ActiveTradingPairs.First().Left;
                var right = AlgorithmSettings.ActiveTradingPairs.First().Right;
                return $"Total alloc: {alloc.GetAllocation(left)}{left} -- {alloc.GetAllocation(right)}{right}";
            }
        }
    }

    /// <summary>
    /// Stud algorithm settings, used for testing.
    /// </summary>
    internal class TemplateAlgorithmSettings : AlgorithmSettings
    {
    }
}

#pragma warning restore SA1402
