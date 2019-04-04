using System;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Object managing the active state and related resources.
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm configuration.</typeparam>
    internal sealed class StateManager<T> : Observable<Type>, IDisposable
        where T : AlgorithmConfiguration
    {
        private readonly object _lock = new object();
        private readonly T _configuration;
        private readonly ILogger _logger;
        private readonly ExchangeProvidersContainer _container;
        private readonly IDisposable _timerObserver;
        private readonly IDisposable _tradingObserver;

        private State<T> _activeState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateManager{T}"/> class.
        /// Sets active state with an initial state and sets basic configuration.
        /// </summary>
        /// <param name="algorithmConfiguration">The configuration of the algorithm.</param>
        /// <param name="container">Exchange service container.</param>
        /// <param name="initial">The initial state.</param>
        public StateManager(
            T algorithmConfiguration,
            ExchangeProvidersContainer container,
            EntryState<T> initial)
        {
            lock (_lock)
            {
                // Setup logging
                _configuration = algorithmConfiguration;
                _container = container;
                _logger = container.LoggerFactory.CreateLogger(GetType());

                // Setup observing
                _timerObserver = container.TimerProvider.Subscribe(
                    new ConfigurableObserver<long>(
                    () => { },
                    _ => { },
                    tick =>
                    {
                        if (_activeState is null)
                        {
                            Activate(initial);
                        }

                        OnMarketConditionEval();
                        EvaluateStateTimer();
                    }));

                _tradingObserver = container.TradingProvider.Subscribe(
                    new ConfigurableObserver<OrderUpdate>(
                    () => { },
                    _ => { },
                    OnOrderUpdateEval));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose the StateManager.
        /// </summary>
        /// <param name="disposing">Actually do it.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _timerObserver.Dispose();
                    _tradingObserver.Dispose();
                    _container.Dispose();
                }
            }
        }

        /// <summary>
        /// Activates the state manager by feeding it the initial state.
        /// </summary>
        /// <param name="initial">The initial state.</param>
        private void Activate(EntryState<T> initial)
        {
            Guard.Argument(initial).NotNull();
            if (_activeState != null)
            {
                throw new InvalidOperationException("Cannot activate the state manager when it is already active.");
            }

            _activeState = initial;
            SwitchState(_activeState.Activate(_configuration, _container));
        }

        /// <summary>
        /// Evaluates the active state's market condition predicate.
        /// </summary>
        private void OnMarketConditionEval()
        {
            lock (_lock)
            {
                try
                {
                    var next = _activeState.OnMarketCondition(_container.DataProvider);
                    SwitchState(next);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Evaluates the active state's order update condition.
        /// </summary>
        /// <param name="order">Update of a certain order.</param>
        private void OnOrderUpdateEval(OrderUpdate order)
        {
            lock (_lock)
            {
                try
                {
                    var next = _activeState.OnOrderUpdate(order);
                    SwitchState(next);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Evaluate the timer of the current state.
        /// </summary>
        private void EvaluateStateTimer()
        {
            lock (_lock)
            {
                try
                {
                    if (!_activeState.TimerTriggered
                        && _activeState.EndTime <= _container.TimerProvider.CurrentTime)
                    {
                        _activeState.TimerTriggered = true;
                        var next = _activeState.OnTimerElapsed();
                        SwitchState(next);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Switches the active state to the given state, only to be used by states.
        /// </summary>
        /// <param name="child">State to switch to.</param>
        /// <exception cref="Exception">Child can't be null.</exception>
        private void SwitchState(State<T> child)
        {
            // This function is safe because it is executed in the locked context of the OnX callback functions
            Guard.Argument(child).NotNull();
            if (child is NothingState<T>)
            {
                return;
            }

            lock (_lock)
            {
                _logger.LogInformation(
                    $"STATE SWITCH: {_activeState.GetType().Name} ---> {child.GetType().Name} at {_container.TimerProvider.CurrentTime}");

                // Full cycle -> increase TradeID
                if (child is EntryState<T>)
                {
                    _container.TradingProvider.IncrementTradeId();
                }

                // Add state switch event to the database
                UpdateObservers(child.GetType());

                _activeState = child;

                // Keep switching if the run method yields a new state.
                var next = _activeState.Activate(_configuration, _container);
                if (!(next is NothingState<T>))
                {
                    SwitchState(next);
                }
            }
        }
    }
}
