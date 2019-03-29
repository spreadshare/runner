using System;
using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain long is within the inclusive range of two numbers.
    /// </summary>
    internal class RangeInt : Constraint
    {
        private readonly int _min;
        private readonly int _max;

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
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if ((int)value < _min || (int)value > _max)
            {
                yield return $"{name} has value {value} which is not in [{_min}, {_max}]";
            }
        }
    }
}
