namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to start the algorithm execution.
    /// </summary>
    internal class CommandStartBot : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_start_bot";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
