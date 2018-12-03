using System.Data.Common;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

#pragma warning disable SA1402

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Stud algorithm, used for testing
    /// </summary>
    internal class TemplateAlgorithm : BaseAlgorithm
    {
        /// <inheritdoc />
        public override ResponseObject Start(AlgorithmSettings settings, ExchangeProvidersContainer container, DatabaseContext database)
        {
            var stateManager = new StateManager<TemplateAlgorithmSettings>(
                settings as TemplateAlgorithmSettings,
                new TemplateState(),
                container,
                database);
            return new ResponseObject(ResponseCode.Success);
        }

        private class TemplateState : State<TemplateAlgorithmSettings>
        {
            protected override void Run(TradingProvider trading, DataProvider data)
            {
                // Increase the trade id every time the entry state is called.
                trading.TradeId += 1;
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
