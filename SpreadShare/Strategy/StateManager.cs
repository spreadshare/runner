using System;
using System.Linq;
using System.Threading;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    class StateManager
    {
        private State _activeState;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public AbstractTradingService TradingService;
        public AbstractUserService UserService;

        private Timer timer;

        
        public string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Constructor: Initialise the active state with an initial state
        /// </summary>
        /// <param name="initial">First state to be active. Can't be null</param>
        /// <param name="loggerFactory">Provides logger for StateManager and states</param>
        public StateManager(State initial, ILoggerFactory loggerFactory, ITradingService tradingService, IUserService userService)
        {
            lock (_lock)
            {
                _logger = loggerFactory.CreateLogger("StateManager");
                _loggerFactory = loggerFactory;
                TradingService = tradingService as AbstractTradingService;
                UserService = userService as AbstractUserService;
                UserService.OrderUpdateHandler += OnOrderUpdate;
                _activeState = initial ?? throw new Exception("Given initial state is null. State manager may only contain non-null states");
                initial.Activate(new Context(), this, _loggerFactory);
            }
        }

        /// <summary>
        /// Switches the active state to the given state
        /// </summary>
        /// <param name="child">State to switch to</param>
        public void SwitchState(State child)
        {
            //This function is safe because it is executed in the locked context of the OnX callback functions
            if (child == null) throw new Exception("Given child state is null. State manager may only contain non-null states");

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            Context c = _activeState.Context;
            Interlocked.Exchange(ref _activeState, child);
            GC.SuppressFinalize(_activeState);
            child.Activate(c, this, _loggerFactory);
        }


        /// <summary>
        /// Example of an action
        /// </summary>
        public void OnCandle(Candle c)
        {
            lock (_lock)
            {
                _activeState.OnCandle(c);
            }
        }

        public void SetTimer(long ms) {
            timer = new Timer(ms, OnTimer);
        }

        private void OnTimer() {
            lock(_lock) {
                //Recheck if the timer has not been resetted by another thread in the meantime
                if (timer.Valid) {
                    try {
                        _activeState.OnTimer();
                    } catch(Exception e) {
                        _logger.LogInformation("Timer callback failed, must have been interrupted.");
                    }
                }
            }
        }

        private void OnOrderUpdate(object sender, BinanceStreamOrderUpdate order) {
            lock (_lock)
            {
                _activeState.OnOrderUpdate(order);
            }
        }
    }
}
