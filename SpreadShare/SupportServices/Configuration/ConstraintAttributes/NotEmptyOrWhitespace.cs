using System;

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
        public override string OnError(string name, object value)
            => $"{name} cannot be empty or whitespace";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            return !string.IsNullOrEmpty((string)value) && !string.IsNullOrWhiteSpace((string)value);
        }
    }
}