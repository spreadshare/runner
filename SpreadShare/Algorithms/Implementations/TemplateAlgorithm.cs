using SpreadShare.ExchangeServices;
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
            return new ResponseObject(ResponseCode.Success);
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
