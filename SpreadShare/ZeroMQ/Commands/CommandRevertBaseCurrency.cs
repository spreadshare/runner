namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to revert to the base currency immediately.
    /// </summary>
    internal class CommandRevertBaseCurrency : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_revert_basecurrency";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
