using SpreadShare.ExchangeServices.Providers;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// Indicator command for the RelativeStrengthIndex.
    /// </summary>
    [CommandName("rsi")]
    internal class RelativeStrengthIndexCommand : IndicatorCommand<decimal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeStrengthIndexCommand"/> class.
        /// </summary>
        /// <param name="input">The argument inputs.</param>
        public RelativeStrengthIndexCommand(params string[] input)
            : base(x => x.RelativeStrengthIndex(), input)
        {
        }
    }
}