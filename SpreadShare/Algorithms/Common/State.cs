using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms.Common
{
    /// <summary>
    /// Base class of a state of a algorithm
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm</typeparam>
    internal abstract class State<T>
        where T : AlgorithmSettings
    {
        private StateManager<T> _stateManager;

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
        /// Gets a link to the parent algorithm settings
        /// </summary>
        protected T AlgorithmSettings { get; private set; }

        /// <summary>
        /// Initialise the state
        /// </summary>
        /// <param name="stateManager">StateManager controlling this state</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public void Activate(StateManager<T> stateManager, ILoggerFactory loggerFactory)
        {
            _stateManager = stateManager;
            TradingService = stateManager.TradingService;
            UserService = stateManager.UserService;
            Logger = loggerFactory.CreateLogger(GetType());
            AlgorithmSettings = _stateManager.AlgorithmSettings;
            Run();
        }

        /// <summary>
        /// Callback when the timer elapses (fired by StateManager)
        /// </summary>
        /// <returns>Whether the specified callback was successful</returns>
        public virtual ResponseObject OnTimer() => new ResponseObject(ResponseCode.NotDefined);

        /// <summary>
        /// Switching states
        /// </summary>
        /// <param name="s">State to switch to</param>
        protected void SwitchState(State<T> s)
        {
            _stateManager.SwitchState(s);
        }

        /// <summary>
        /// Validates if all the required parameters exist within the context
        /// </summary>
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
