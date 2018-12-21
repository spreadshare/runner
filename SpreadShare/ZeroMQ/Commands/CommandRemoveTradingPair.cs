using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to remove a trading pair.
    /// </summary>
    internal class CommandRemoveTradingPair : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_remove_tradingpair";

        /// <inheritdoc />
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            { "arg_tradingpair", null }
        };

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
