using System;
using System.Globalization;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain long is within the inclusive range of two numbers.
    /// </summary>
    internal class RangeDecimal : Constraint
    {
        private readonly decimal _min;
        private readonly decimal _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeDecimal"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeDecimal(string min, string max)
        {
            _min = decimal.Parse(min, CultureInfo.InvariantCulture);
            _max = decimal.Parse(max, CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(decimal);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} was not in range {_min}, {_max}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => (decimal)value >= _min && (decimal)value <= _max;
    }
}
