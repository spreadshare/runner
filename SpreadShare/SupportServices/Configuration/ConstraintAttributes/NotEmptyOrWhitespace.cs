using System;
using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a string property is not empty or whitespace.
    /// </summary>
    internal class NotEmptyOrWhitespace : Constraint
    {
        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (string.IsNullOrEmpty((string)value) || string.IsNullOrWhiteSpace((string)value))
            {
                yield return $"{name} cannot be empty or whitespace";
            }
        }
    }
}