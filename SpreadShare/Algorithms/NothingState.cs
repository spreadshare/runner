using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Class representing a non existing state.
    /// </summary>
    /// <typeparam name="T">Kind of algorithm settings.</typeparam>
    internal sealed class NothingState<T> : State<T>
        where T : AlgorithmConfiguration
    {
        /// <inheritdoc />
        protected override State<T> Run(TradingProvider trading, DataProvider data)
        {
            throw new InvalidOperationException("Nothing state should not be executed");
        }
    }
}