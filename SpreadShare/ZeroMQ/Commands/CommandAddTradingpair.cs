using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadShare.ZeroMQ
{
    class CommandAddTradingpair : Command
    {
        protected override string CommandString => "command_add_tradingpair";
        protected override Dictionary<string, string> Arguments => new Dictionary<string, string>()
        {
            {"arg_tradingpair", null}
        };

        public override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
