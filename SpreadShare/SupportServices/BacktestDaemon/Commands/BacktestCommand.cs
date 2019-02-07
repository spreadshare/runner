using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// The abstract definition of a backtest command.
    /// </summary>
    [CommandDescription("no description")]
    [CommandArguments("")]
    internal abstract class BacktestCommand
    {
        // Enforce that implementation use this constructor.
        #pragma warning disable CA1801
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestCommand"/> class.
        /// </summary>
        /// <param name="inputs">inputs.</param>
        protected BacktestCommand(params string[] inputs)
        {
        }
        #pragma warning restore CA1801

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="state">The state of the backtest daemon.</param>
        public abstract void Execute(BacktestDaemonState state);
    }
}