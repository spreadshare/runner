using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy
{
    internal abstract class BaseStrategy : IStrategy
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITradingService _tradingService;
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;
        
        public StateManager StateManager { get; private set; }

       
        /// BaseConstrcutor: Provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory">Provided logger creating capabilities</param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user data fetching capabilities</param>
        /// <param name="settingsService">Provides acces to global settings</param>
        protected BaseStrategy(ILoggerFactory loggerFactory,
            ITradingService tradingService, IUserService userService, ISettingsService settingsService)
        {
            _loggerFactory = loggerFactory; 
            _tradingService = tradingService;
            _userService = userService;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Start strategy with the initial state using a StateManager
        /// </summary>
        public ResponseObject Start()
        {
            StateManager = new StateManager(
                GetInitialState(), 
                _loggerFactory, 
                _tradingService,
                _userService,
                _settingsService
            );
            return new ResponseObject(ResponseCodes.Success);
        }

        public abstract State GetInitialState();
    }
}
