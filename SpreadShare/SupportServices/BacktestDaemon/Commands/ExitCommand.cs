using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// The exit command.
    /// </summary>
    [CommandName("exit")]
    [CommandName("quit")]
    [CommandDescription("exit the application")]
    internal class ExitCommand : BacktestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitCommand"/> class.
        /// </summary>
        /// <param name="inputs">inputs.</param>
        public ExitCommand(params string[] inputs)
            : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override void Execute(BacktestDaemonState state)
        {
            Program.ExitProgramWithCode(ExitCode.UserShutdown);
        }
    }
}