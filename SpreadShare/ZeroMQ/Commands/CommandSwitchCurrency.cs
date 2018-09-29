using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    internal class CommandSwitchCurrency : Command
    {
        protected override string CommandString => "command_switch_currency";
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            {"arg_currency", null}
        };

        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
