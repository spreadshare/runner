namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to retrieve the HoldTime
    /// </summary>
    internal class CommandGetHoldtime : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_stop_bot";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
