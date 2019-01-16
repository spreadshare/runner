using System;
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
                Logger.LogInformation("I wonder if Miss Bitcoin think I should buy...");
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
