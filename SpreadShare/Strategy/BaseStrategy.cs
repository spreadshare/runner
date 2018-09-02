using Microsoft.Extensions.Logging;

namespace SpreadShare.Strategy
{
    abstract class BaseStrategy : IStrategy
    {
        private readonly ILoggerFactory _loggerFactory;
        public StateManager StateManager { get; private set; }

        /// <summary>
        /// BaseConstrcutor: Provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory"></param>
        protected BaseStrategy(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Start strategy with initial state using a StateManager
        /// </summary>
        public void Start()
        {
            StateManager = new StateManager(GetInitialState(), _loggerFactory);
        }

        public abstract State GetInitialState();
    }
}
