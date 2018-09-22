using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    abstract class BaseStrategy : IStrategy
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITradingService _tradingService;
        private readonly IUserService _userService;
        public StateManager StateManager { get; private set; }

        /// <summary>
        /// BaseConstrcutor: Provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user watching capabilities</param>
        protected BaseStrategy(ILoggerFactory loggerFactory,
            ITradingService tradingService, IUserService userService)
        {
            _loggerFactory = loggerFactory;
            _tradingService = tradingService;
            _userService = userService;
        }

        /// <summary>
        /// Start strategy with initial state using a StateManager
        /// </summary>
        public ResponseObject Start()
        {
            StateManager = new StateManager(
                GetInitialState(), 
                _loggerFactory, 
                _tradingService, 
                _userService
            );
            return new ResponseObject(ResponseCodes.Success);
        }

        public abstract State GetInitialState();
    }
}
