namespace SpreadShare.ZeroMQ.Commands
{
    /// <summary>
    /// Command to retrieve the CheckTime
    /// </summary>
    internal class CommandGetChecktime : Command
    {
        /// <inheritdoc />
        protected override string CommandString => "command_get_checktime";

        /// <inheritdoc />
        public override void Action()
        {
            throw new System.NotImplementedException();
        }
    }
}
