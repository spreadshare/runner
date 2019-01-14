using System;
using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to add a trading pair.
    /// </summary>
    internal class CommandAddTradingpair : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_add_tradingpair";

        /// <inheritdoc />
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            { "arg_tradingpair", null },
        };

        /// <inheritdoc />
        public override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
