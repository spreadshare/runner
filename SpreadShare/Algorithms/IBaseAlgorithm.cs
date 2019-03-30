using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.Configuration;

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
        /// <param name="configuration">Provides access to configuration of the algorithm.</param>
        /// <param name="container">Provides trading and data gathering capabilities.</param>
        /// <returns>Whether the algorithm started successfully.</returns>
        ResponseObject Start(AlgorithmConfiguration configuration, ExchangeProvidersContainer container);
    }
}