using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to switch to a different currency.
    /// </summary>
    internal class CommandSwitchCurrency : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_switch_currency";

        /// <inheritdoc />
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            { "arg_currency", null }
        };

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
