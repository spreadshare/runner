using Dawn;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class for all algorithms.
    /// </summary>
    /// <typeparam name="T">The derived AlgorithmSettings.</typeparam>
    internal abstract class BaseAlgorithm<T> : IBaseAlgorithm
        where T : AlgorithmSettings
    {
        /// <summary>
        /// Gets the the EntryState of the algorithm.
        /// </summary>
        protected abstract EntryState<T> Initial { get; }

        private StateManager<T> StateManager { get; set; }

        /// <inheritdoc />
        public ResponseObject Start(AlgorithmSettings settings, ExchangeProvidersContainer container, DatabaseContext database)
        {
            Guard.Argument(settings).Require(
                x => x is T,
                x => $"{x} cannot not be converted to {typeof(T)}, please make sure to use the correct AlgorithmSettings");
            Start(settings as T, container, database);
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public ResponseObject Stop()
        {
            StateManager.Dispose();
            return new ResponseObject(ResponseCode.Success);
        }

        private void Start(T settings, ExchangeProvidersContainer container, DatabaseContext database)
            => StateManager = new StateManager<T>(settings, Initial, container, database);
    }
}
