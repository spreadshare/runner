using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to change the CheckTime.
    /// </summary>
    internal class CommandChangeChecktime : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_change_checktime";

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
