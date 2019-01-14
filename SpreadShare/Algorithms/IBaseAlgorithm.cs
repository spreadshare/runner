using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Interface for any algorithm.
    /// </summary>
    internal interface IBaseAlgorithm
    {
        /// <summary>
        /// Start algorithm with the initial state using a StateManager.
        /// </summary>
        /// <param name="settings">Provides access to settings of the algorithm.</param>
        /// <param name="container">Provides trading and data gathering capabilities.</param>
        /// <param name="database">The database context.</param>
        /// <returns>Whether the algorithm started successfully.</returns>
        ResponseObject Start(AlgorithmSettings settings, ExchangeProvidersContainer container, DatabaseContext database);

        /// <summary>
        /// Stops the algorithm.
        /// </summary>
        /// <returns>Whether the algorithm was stopped successfully.</returns>
        ResponseObject Stop();
    }
}