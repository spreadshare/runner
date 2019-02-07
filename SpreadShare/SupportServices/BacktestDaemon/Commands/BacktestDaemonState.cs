using Dawn;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices.Allocation;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// Defines the state object passed during the Backtest cli.
    /// </summary>
    internal class BacktestDaemonState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDaemonState"/> class.
        /// </summary>
        /// <param name="algoService">The algorithm service.</param>
        /// <param name="allocationManager">The allocation manager.</param>
        public BacktestDaemonState(AlgorithmService algoService, AllocationManager allocationManager)
        {
            Guard.Argument(algoService).NotNull();
            AlgorithmService = algoService;
            AllocationManager = allocationManager;
        }

        /// <summary>
        /// Gets the algorithm service.
        /// </summary>
        public AlgorithmService AlgorithmService { get; }

        /// <summary>
        /// Gets the allocation manager.
        /// </summary>
        public AllocationManager AllocationManager { get; }

        /// <summary>
        /// Gets or sets the ID of the current backtest.
        /// </summary>
        public int CurrentBacktestID { get; set; }

        /// <summary>
        /// Gets or sets the configuration path of the current backtest.
        /// </summary>
        public string CurrentBacktestConfigurationPath { get; set; }
    }
}