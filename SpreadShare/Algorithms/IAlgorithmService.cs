using System;
using SpreadShare.Models;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Interface for services managing the algorithms.
    /// </summary>
    internal interface IAlgorithmService
    {
        /// <summary>
        /// Starts the algorithm using a custom configuration object.
        /// </summary>
        /// <param name="configuration">Configuration object.</param>
        /// <typeparam name="T">The type of algorithm to start.</typeparam>
        /// <returns>If the algorithm was started successfully.</returns>
        ResponseObject StartAlgorithm<T>(AlgorithmConfiguration configuration)
            where T : IBaseAlgorithm;

        /// <summary>
        /// Starts the algorithm using a custom configuration object.
        /// </summary>
        /// <param name="algorithm">Algorithm to start.</param>
        /// <param name="configuration">Configuration object.</param>
        /// <returns>If the algorithm was started successfully.</returns>
        ResponseObject StartAlgorithm(Type algorithm, AlgorithmConfiguration configuration);

        /// <summary>
        /// Stops the given algorithm.
        /// </summary>
        /// <param name="algorithmType">Algorithm to stop.</param>
        /// <returns>If the algorithm was stopped successfully.</returns>
        ResponseObject StopAlgorithm(Type algorithmType);
    }
}
