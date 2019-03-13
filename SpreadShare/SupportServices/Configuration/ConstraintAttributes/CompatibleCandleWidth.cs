using System;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain candle width value is compatible with the configured value.
    /// Compatible meaning, that it is divisible by (and equal/larger) the configured value.
    /// </summary>
    internal class CompatibleCandleWidth : Constraint
    {
        /// <inheritdoc/>
        protected override Type InputType => typeof(CandleWidth);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} has value {value}, which cannot be used in conjunction with the configured {Configuration.Instance.CandleWidth}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => (int)value >= (int)Configuration.Instance.CandleWidth
               && (int)value % (int)Configuration.Instance.CandleWidth == 0;
    }
}