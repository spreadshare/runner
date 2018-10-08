namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to retrieve the active trading pairs
    /// </summary>
    internal class CommandGetTradingpairs : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_get_tradingpairs";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
