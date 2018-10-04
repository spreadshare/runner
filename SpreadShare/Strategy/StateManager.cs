using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy
{
    internal class StateManager
    {
        private State _activeState;
        private Timer _activeTimer;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public AbstractTradingService TradingService;
        public AbstractUserService UserService;
        public SettingsService SettingsService;


        public string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Constructor: Initialise the active state with an initial state and give basic settings
        /// </summary>
        public StateManager(State initial, ILoggerFactory loggerFactory, 
            ITradingService tradingService, IUserService userService, ISettingsService settingsService)
        {
            lock (_lock)
            {
                // Setup logging
                _logger = loggerFactory.CreateLogger("StateManager");
                _loggerFactory = loggerFactory;

                // Setup trading services (gain access to abstract members)
                TradingService = tradingService as AbstractTradingService;
                UserService = userService as AbstractUserService;
                SettingsService = settingsService as SettingsService;

                // Setup initial state
                _activeState = initial ?? throw new Exception("Given initial state is null. State manager may only contain non-null states");
                initial.Activate(new Context(), this, _loggerFactory);
            }
        }

        /// <summary>
        /// Switches the active state to the given state, only to be used by states
        /// </summary>
        /// <param name="child">State to switch to</param>
        public void SwitchState(State child)
        {
            //This function is safe because it is executed in the locked context of the OnX callback functions
            if (child == null) throw new Exception("Given child state is null. State manager may only contain non-null states");

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            var c = _activeState.Context;
            Interlocked.Exchange(ref _activeState, child);
            child.Activate(c, this, _loggerFactory);
        }

        /// <summary>
        /// Creates new Timer object that waits and then executes callback
        /// </summary>
        /// <param name="ms">Time to wait</param>
        public void SetTimer(long ms)
        {
            //Ensure the previous timer has gone out.
            _activeTimer?.Stop();
            _activeTimer = new Timer(ms, () =>
            {
                // Callback returned after waiting period
                lock (_lock)
                {
                    /* State.OnTimer should return Success, while states without implementing a timer
                     * will return NotDefined by default.
                    */
                    var response = _activeState.OnTimer();
                    if (!response.Success) {
                        _logger.LogInformation($"Timer callback was not used by state. Response Code: {response}");
                    }   
                }
            } );
        }
    }
}
