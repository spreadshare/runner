namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to retrieve the base currency.
    /// </summary>
    internal class CommandGetBasecurrency : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_get_basecurrency";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
