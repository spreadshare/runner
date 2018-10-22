using System.Data;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class for all algorithms
    /// </summary>
    /// <typeparam name="T">The specific algorithm that is associated with it</typeparam>
    internal abstract class BaseAlgorithm
    {
        /// <summary>
        /// Start algorithm with the initial state using a StateManager
        /// </summary>
        /// <returns>Whether the algorithm started succesfully</returns>
        public abstract ResponseObject Start(ILoggerFactory loggerFactory,
            ISettingsService settingsService, ExchangeProvidersContainer container);

        public abstract System.Type GetSettingsType { get; }

    }
}
