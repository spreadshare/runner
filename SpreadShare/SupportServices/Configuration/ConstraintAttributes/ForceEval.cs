using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Enforces that read only properties are validated.
    /// </summary>
    internal class ForceEval : Constraint
    {
        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            yield break;
        }
    }
}