using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    internal class CommandChangeBasecurrency : Command
    {
        protected override string CommandString => "command_change_basecurrency";
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
