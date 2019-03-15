using System;
using Dawn;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Base class for all algorithms.
    /// </summary>
    /// <typeparam name="T">The derived AlgorithmConfiguration.</typeparam>
    internal abstract class BaseAlgorithm<T> : IBaseAlgorithm
        where T : AlgorithmConfiguration
    {
        /// <summary>
        /// Gets the the EntryState of the algorithm.
        /// </summary>
        protected abstract EntryState<T> Initial { get; }

        private StateManager<T> StateManager { get; set; }

        /// <inheritdoc />
        public ResponseObject Start(AlgorithmConfiguration configuration, ExchangeProvidersContainer container)
        {
            Guard.Argument(configuration).Require(
                x => x is T,
                x => $"{x} cannot not be converted to {typeof(T)}, please make sure to use the correct AlgorithmConfiguration");
            Start(configuration as T, container);
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public ResponseObject Stop()
        {
            try
            {
                StateManager.Dispose();
            }
            catch (Exception e)
            {
                return new ResponseObject(ResponseCode.Error, e.Message);
            }

            return new ResponseObject(ResponseCode.Success);
        }

        private void Start(T configuration, ExchangeProvidersContainer container)
        {
            StateManager = new StateManager<T>(configuration, container, Initial);
            DatabaseEventListenerService.Instance?.AddDataSource(StateManager);
            container.TimerProvider.RunPeriodicTimer();
        }
    }
}
