﻿namespace SpreadShare.ZeroMQ.Commands
{
    internal class CommandResetHoldingTime : Command
    {
        protected override string CommandString => "command_reset_holding_time";
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}