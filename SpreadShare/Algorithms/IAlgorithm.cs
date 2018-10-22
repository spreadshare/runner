using SpreadShare.Models;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Interface for all algorithms
    /// </summary>
    internal interface IAlgorithm
    {
        /// <summary>
        /// Starts the algorithm
        /// </summary>
        /// <returns>Whether the algorithm was started succesfully</returns>
        ResponseObject Start();
    }
}
