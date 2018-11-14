using System;
using System.Linq;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Object managing the active state and related resources
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm settings</typeparam>
    internal class StateManager<T>
        where T : AlgorithmSettings
    {
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConfigurableObserver<long> _periodicObserver;

        private State<T> _activeState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateManager{T}"/> class.
        /// Sets active state with an initial state and sets basic settings
        /// </summary>
        /// <param name="algorithmSettings">The settings of the algorithm settings</param>
        /// <param name="initial">Initial state of the algorithm</param>
        /// <param name="container">Exchange service container</param>
        public StateManager(
            T algorithmSettings,
            State<T> initial,
            ExchangeProvidersContainer container)
        {
            Guard.Argument(initial).NotNull();
            lock (_lock)
            {
                // Setup logging
                _logger = container.LoggerFactory.CreateLogger("StateManager");
                _loggerFactory = container.LoggerFactory;

                // Link the parent algorithm setting
                AlgorithmSettings = algorithmSettings;

                Container = container;

                // Setup observing
                _periodicObserver = new ConfigurableObserver<long>(
                    time => OnMarketConditionEval(),
                    () => { },
                    e => { });
                container.TimerProvider.Subscribe(_periodicObserver);

                // Setup initial state
                _activeState = initial;
                _activeState.Activate(algorithmSettings, Container.TradingProvider, _loggerFactory);
            }
        }

        /// <summary>
        /// Gets the container with exchange service providers
        /// </summary>
        private ExchangeProvidersContainer Container { get; }

        /// <summary>
        /// Gets a link to the algorithm settings.
        /// </summary>
        private T AlgorithmSettings { get; }

        /// <summary>
        /// Gets the current active state
        /// </summary>
        private string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Evaluates the active state's market condition predicate
        /// </summary>
        public void OnMarketConditionEval()
        {
            lock (_lock)
            {
                var next = _activeState.OnMarketCondition(Container.DataProvider);
                SwitchState(next);
            }
        }

        /// <summary>
        /// Evaluates the active state's order update condition.
        /// </summary>
        /// <param name="order">update of a certain order</param>
        public void OnOrderUpdateEval(OrderUpdate order)
        {
            lock (_lock)
            {
                var next = _activeState.OnOrderUpdate(order);
                SwitchState(next);
            }
        }

        /// <summary>
        /// Switches the active state to the given state, only to be used by states
        /// </summary>
        /// <param name="child">State to switch to</param>
        /// <exception cref="Exception">Child can't be null</exception>
        private void SwitchState(State<T> child)
        {
            // This function is safe because it is executed in the locked context of the OnX callback functions
            Guard.Argument(child).NotNull();
            if (child is NothingState<T>)
            {
                return;
            }

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            Interlocked.Exchange(ref _activeState, child);

            _activeState.Activate(AlgorithmSettings, Container.TradingProvider, _loggerFactory);
        }
    }
}
