using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    class CommandChangeChecktime : Command
    {
        protected override string CommandString => "command_change_checktime";
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
