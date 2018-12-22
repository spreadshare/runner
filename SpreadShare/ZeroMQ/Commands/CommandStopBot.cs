namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to stop the algorithm execution.
    /// </summary>
    internal class CommandStopBot : Command
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
