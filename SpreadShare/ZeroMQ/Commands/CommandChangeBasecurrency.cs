using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to change the base currency.
    /// </summary>
    internal class CommandChangeBasecurrency : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_change_basecurrency";

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
