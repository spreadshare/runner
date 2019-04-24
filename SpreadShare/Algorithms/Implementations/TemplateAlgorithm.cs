using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using Config = SpreadShare.Algorithms.Implementations.TemplateAlgorithmConfiguration;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Stud algorithm, used for testing.
    /// </summary>
    internal class TemplateAlgorithm : BaseAlgorithm<Config>
    {
        /// <inheritdoc />
        protected override EntryState<Config> Initial => new WelcomeState();

        private class WelcomeState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("Welcome to the TemplateAlgorithm");
                return new TemplateState();
            }
        }

        private class TemplateState : EntryState<Config>
        {
            protected override State<Config> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("I wonder if Miss Bitcoin thinks I should buy...");
                Logger.LogInformation(ShowAlloc(trading));
                WaitForNextCandle();
                Logger.LogInformation(ShowAlloc(trading));
                return new ZombieState();
            }

            private string ShowAlloc(TradingProvider trading)
            {
                var alloc = trading.GetPortfolio();
                var left = AlgorithmConfiguration.TradingPairs.First().Left;
                var right = AlgorithmConfiguration.TradingPairs.First().Right;
                return $"Total alloc: {alloc.GetAllocation(left)}{left} -- {alloc.GetAllocation(right)}{right}";
            }
        }

        private class ZombieState : State<TemplateAlgorithmConfiguration>
        {
            protected override State<TemplateAlgorithmConfiguration> Run(TradingProvider trading, DataProvider data)
            {
                Logger.LogInformation("I desire to expire.");
                return new NothingState<TemplateAlgorithmConfiguration>();
            }
        }
    }

    /// <summary>
    /// Stud algorithm settings, used for testing.
    /// </summary>
    internal class TemplateAlgorithmConfiguration : AlgorithmConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateAlgorithmConfiguration"/> class.
        /// </summary>
        public TemplateAlgorithmConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateAlgorithmConfiguration"/> class.
        /// </summary>
        /// <param name="pairs">The trading pairs.</param>
        /// <param name="candleWidth">The candle width.</param>
        public TemplateAlgorithmConfiguration(IEnumerable<TradingPair> pairs, int candleWidth)
        {
            __tradingPairs = pairs.Select(x => x.ToString()).ToList();
            CandleWidth = candleWidth;
        }
    }
}

#pragma warning restore SA1402
