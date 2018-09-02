using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Strategy
{
    class StateManager
    {
        private State _activeState;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Constructor: Initialise the active state with an initial state
        /// </summary>
        /// <param name="initial">First state to be active. Can't be null</param>
        /// <param name="loggerFactory">Provides logger for StateManager and states</param>
        public StateManager(State initial, ILoggerFactory loggerFactory)
        {
            lock (_lock)
            {
                _logger = loggerFactory.CreateLogger("StateManager");
                _loggerFactory = loggerFactory;
                _activeState = initial ?? throw new Exception("Given initial state is null. State manager may only contain non-null states");
                initial.Activate(null, this, _loggerFactory);
            }
        }

        /// <summary>
        /// Switches the active state to the given state
        /// </summary>
        /// <param name="child">State to switch to</param>
        public void SwitchState(State child)
        {
            if (child == null) throw new Exception("Given child state is null. State manager may only contain non-null states");

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            Context c = _activeState.Context;
            Interlocked.Exchange(ref _activeState, child);
            child.Activate(c, this, _loggerFactory);
        }


        /// <summary>
        /// Example of an action
        /// </summary>
        public void OnSomeAction()
        {
            lock (_lock)
            {
                _activeState.OnSomeAction();
            }
        }
    }
}
