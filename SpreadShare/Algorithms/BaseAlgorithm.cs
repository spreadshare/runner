using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class for all algorithms
    /// </summary>
    internal abstract class BaseAlgorithm
    {
        /// <summary>
        /// Gets type of the settings of the algorithm
        /// </summary>
        public abstract System.Type GetSettingsType { get; }

        /// <summary>
        /// Start algorithm with the initial state using a StateManager
        /// </summary>
        /// <param name="settings">Provides access to settings of the algorithm</param>
        /// <param name="container">Provides trading and data gathering capabilities</param>
        /// <returns>Whether the algorithm started succesfully</returns>
        public abstract ResponseObject Start(
            AlgorithmSettings settings,
            ExchangeProvidersContainer container);
    }
}
