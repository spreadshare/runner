using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Stud algorithm, used for testing
    /// </summary>
    internal class TemplateAlgorithm : BaseAlgorithm<TemplateAlgorithmSettings>
    {
        /// <inheritdoc />
        protected override EntryState<TemplateAlgorithmSettings> Initial => new TemplateState();

        private class TemplateState : EntryState<TemplateAlgorithmSettings>
        {
            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }
    }

    /// <summary>
    /// Stud algorithm settings, used for testing
    /// </summary>
    internal class TemplateAlgorithmSettings : AlgorithmSettings
    {
    }
}

#pragma warning restore SA1402
