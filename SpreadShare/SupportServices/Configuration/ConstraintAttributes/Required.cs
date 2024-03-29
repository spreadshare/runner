using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Requires a setting to be not null.
    /// </summary>
    internal class Required : Constraint
    {
        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (value == null)
            {
                yield return $"{name} must have a value";
            }
        }
    }
}