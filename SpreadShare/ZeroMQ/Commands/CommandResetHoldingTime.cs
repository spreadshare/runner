namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to reset the remaining holding time to HoldTime.
    /// </summary>
    internal class CommandResetHoldingTime : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_reset_holding_time";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
