using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    internal class CommandChangeHoldtime : Command
    {
        protected override string CommandString => "command_change_holdtime";
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            {"arg_time", null}
        };

        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
