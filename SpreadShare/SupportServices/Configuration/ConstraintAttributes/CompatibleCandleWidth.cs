using System;
using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Enforces that a number is a multiple of the global candle width property.
    /// </summary>
    internal class CompatibleCandleWidth : Constraint
    {
        /// <inheritdoc/>
        protected override Type InputType { get; } = typeof(int);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            int val = (int)value;
            if (val % Configuration.Instance.CandleWidth != 0 || val <= 0)
            {
                yield return
                    $"{name} has value {value} which is not a multiple of the configured width of {Configuration.Instance.CandleWidth}m";
            }
        }
    }
}