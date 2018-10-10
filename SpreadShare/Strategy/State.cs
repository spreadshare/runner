using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Base class of a state of a strategy
    /// </summary>
    internal abstract class State
    {
        private StateManager _stateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        protected State()
        {
        }

        /// <summary>
        /// Gets the logger of the state
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets a trading service instance
        /// </summary>
        protected AbstractTradingService TradingService { get; private set; }

        /// <summary>
        /// Gets a user service instance
        /// </summary>
        protected AbstractUserService UserService { get; private set; }

        /// <summary>
        /// Gets a setting service instance
        /// </summary>
        protected SettingsService SettingsService { get; private set; }

        /// <summary>
        /// Initialise the state
        /// </summary>
        /// <param name="stateManager">StateManager controlling this state</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public void Activate(StateManager stateManager, ILoggerFactory loggerFactory)
        {
            _stateManager = stateManager;
            TradingService = stateManager.TradingService;
            UserService = stateManager.UserService;
            SettingsService = stateManager.SettingsService;
            Logger = loggerFactory.CreateLogger(GetType());
            Run();
        }

        /// <summary>
        /// Callback when the timer elapses (fired by StateManager)
        /// </summary>
        /// <returns>Whether the specified callback was successful</returns>
        public virtual ResponseObject OnTimer() => new ResponseObject(ResponseCodes.NotDefined);

        /// <summary>
        /// Switching states
        /// </summary>
        /// <param name="s">State to switch to</param>
        protected void SwitchState(State s)
        {
            _stateManager.SwitchState(s);
        }

        /// <summary>
        /// Validates if all the required parameters exist within the context
        /// </summary>
        /// TODO: The current name does not reflect what the method does
        protected abstract void Run();

        /// <summary>
        /// Sets the timer in the StateManager
        /// </summary>
        /// <param name="ms">Timer duration</param>
        protected void SetTimer(uint ms)
        {
            _stateManager.SetTimer(ms);
        }
    }
}
