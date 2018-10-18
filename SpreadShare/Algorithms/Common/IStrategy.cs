﻿using SpreadShare.Models;

namespace SpreadShare.Algorithms.Common
{
    /// <summary>
    /// Interface for all algorithms
    /// </summary>
    internal interface IStrategy
    {
        /// <summary>
        /// Starts the algorithm
        /// </summary>
        /// <returns>Whether the algorithm was started succesfully</returns>
        ResponseObject Start();
    }
}
