using System.Linq;
using System.Reflection;
using ConsoleTables;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;
using SpreadShare.Utilities;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// Shows all the possible commands.
    /// </summary>
    [CommandName("help")]
    [CommandDescription("show this overview")]
    internal class HelpCommand : BacktestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommand"/> class.
        /// </summary>
        /// <param name="inputs">inputs.</param>
        public HelpCommand(params string[] inputs)
            : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override void Execute(BacktestDaemonState state)
        {
            var allCommands = Reflections.GetAllSubtypes(typeof(BacktestCommand));

            // Get a list of tuples -> [(name(s), args, description)]
            var overview = allCommands
                .Select(
                    c => (string.Join(", ", c.GetCustomAttributes(true)
                            .OfType<CommandName>().Select(x => x.Value)),
                        string.Join(",", c.GetCustomAttribute<CommandArguments>().Values),
                        c.GetCustomAttribute<CommandDescription>().Value));

            var table = new ConsoleTable("command", "arguments", "description");
            foreach (var (names, args, description) in overview)
            {
                table.AddRow(names, args, description);
            }

            table.Write(Format.MarkDown);
        }
    }
}