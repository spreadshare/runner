using System;
using SpreadShare.Models;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Interface for services managing the algorithms
    /// </summary>
    internal interface IAlgorithmService
    {
        /// <summary>
        /// Starts the algorithm
        /// </summary>
        /// <param name="algorithm">Algorithm to start</param>
        /// <returns>If the algorithm was started successfully</returns>
        ResponseObject StartAlgorithm(Type algorithm);

        /// <summary>
        /// Stops the given algorithm
        /// </summary>
        /// <param name="algorithm">Algorithm to stop</param>
        /// <returns>If the algorithm was stopped successfully</returns>
        ResponseObject StopAlgorithm(Type algorithm);
    }
}
