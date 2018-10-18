using SpreadShare.Models;

namespace SpreadShare.Algorithms.Common
{
    /// <summary>
    /// Interface for all strategies
    /// </summary>
    internal interface IStrategy
    {
        /// <summary>
        /// Starts the strategy
        /// </summary>
        /// <returns>Whether the strategy was started succesfully</returns>
        ResponseObject Start();
    }
}
