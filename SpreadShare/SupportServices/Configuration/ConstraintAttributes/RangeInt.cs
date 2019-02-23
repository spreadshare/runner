using System;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain long is within the inclusive range of two numbers.
    /// </summary>
    internal class RangeInt : Constraint
    {
        private int _min;
        private int _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeInt"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeInt(int min, int max)
        {
            _min = min;
            _max = max;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(int);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} was not in range {_min}, {_max}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => (int)value >= _min && (int)value <= _max;
    }
}
