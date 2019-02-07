using System;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain value is within the inclusive range of two numbers.
    /// </summary>
    internal class Range : Constraint
    {
        private long _min;
        private long _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public Range(long min, long max)
        {
            _min = min;
            _max = max;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(long);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} was not in range {_min}, {_max}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => (long)value >= _min && (long)value <= _max;
    }
}