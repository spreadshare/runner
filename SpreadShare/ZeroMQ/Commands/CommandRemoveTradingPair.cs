using System.Collections.Generic;

namespace SpreadShare.ZeroMQ.Commands
{
    class CommandRemoveTradingPair : Command
    {
        protected override string CommandString => "command_remove_tradingpair";
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            {"arg_tradingpair", null}
        };

        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
