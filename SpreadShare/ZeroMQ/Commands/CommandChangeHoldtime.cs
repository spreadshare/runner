using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to change the HoldTime.
    /// </summary>
    internal class CommandChangeHoldtime : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_change_holdtime";

        /// <inheritdoc />
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            { "arg_time", null }
        };

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
