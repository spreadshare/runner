using System;
using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain long is within the inclusive range of two numbers.
    /// </summary>
    internal class RangeLong : Constraint
    {
        private readonly decimal _min;
        private readonly decimal _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeLong"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeLong(long min, long max)
        {
            _min = min;
            _max = max;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(long);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if ((long)value < _min || (long)value > _max)
            {
                yield return $"{name} has value {value} which is not in [{_min}, {_max}]";
            }
        }
    }
}
