using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Stud algorithm, used for testing.
    /// </summary>
    internal class TemplateAlgorithm : BaseAlgorithm<TemplateAlgorithmConfiguration>
    {
        /// <inheritdoc />
        protected override EntryState<TemplateAlgorithmConfiguration> Initial => new WelcomeState();

        private class WelcomeState : EntryState<TemplateAlgorithmConfiguration>
        {
            protected override State<TemplateAlgorithmConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("Welcome to the TemplateAlgorithm");
                return new TemplateState();
            }
        }

        private class TemplateState : EntryState<TemplateAlgorithmConfiguration>
        {
            protected override State<TemplateAlgorithmConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("I wonder if Miss Bitcoin thinks I should buy...");
                Logger.LogInformation(ShowAlloc(trading));
                return new NothingState<TemplateAlgorithmConfiguration>();
            }

            private string ShowAlloc(TradingProvider trading)
            {
                var alloc = trading.GetPortfolio();
                var left = AlgorithmConfiguration.TradingPairs.First().Left;
                var right = AlgorithmConfiguration.TradingPairs.First().Right;
                return $"Total alloc: {alloc.GetAllocation(left)}{left} -- {alloc.GetAllocation(right)}{right}";
            }
        }
    }

    /// <summary>
    /// Stud algorithm settings, used for testing.
    /// </summary>
    internal class TemplateAlgorithmConfiguration : AlgorithmConfiguration
    {
    }
}

#pragma warning restore SA1402
