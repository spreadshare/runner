using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy
{
    internal abstract class State
    {
        public Context Context { get; set; }

        private StateManager _stateManager;
        protected ILogger Logger;
        protected AbstractTradingService TradingService;
        protected AbstractUserService UserService;
        protected SettingsService SettingsService;

        protected State()
        {
            Context = new Context();
        }

        /// <summary>
        /// Initialise the state
        /// </summary>
        /// <param name="context">Set of objects that are required for the state to work</param>
        /// <param name="stateManager"></param>
        /// <param name="loggerFactory"></param>
        public void Activate(Context context, StateManager stateManager, ILoggerFactory loggerFactory)
        {
            Context = context;
            _stateManager = stateManager;
            TradingService = stateManager.TradingService;
            UserService = stateManager.UserService;
            SettingsService = stateManager.SettingsService;
            Logger = loggerFactory.CreateLogger(GetType());
            ValidateContext();
        }

        /// <summary>
        /// Validates if all the required parameters exist within the context
        /// </summary>
        protected abstract void ValidateContext();


        /// <summary>
        /// Switching states
        /// </summary>
        /// <param name="s">State to switch to</param>
        protected void SwitchState(State s)
        {
            _stateManager.SwitchState(s);
        }

        protected void SetTimer(long ms) {
            _stateManager.SetTimer(ms);
        }

        public virtual ResponseObject OnTimer() {
            return new ResponseObject(ResponseCodes.NotDefined);
        }
    }
}
