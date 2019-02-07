using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// State that is an entry state, will cause the TradeID to increment.
    /// </summary>
    /// <typeparam name="T">The type of AlgorithmConfiguration.</typeparam>
    internal abstract class EntryState<T> : State<T>
        where T : AlgorithmConfiguration
    {
    }
}