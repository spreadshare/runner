using System;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Class representing a non existing state.
    /// </summary>
    /// <typeparam name="T">Kind of algorithm settings.</typeparam>
    internal sealed class NothingState<T> : State<T>
        where T : AlgorithmSettings
    {
        /// <inheritdoc />
        protected override void Run(TradingProvider trading, DataProvider data)
        {
            throw new InvalidOperationException("Nothing state should not be executed");
        }
    }
}