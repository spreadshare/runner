using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Ensures that a collection is not empty.
    /// </summary>
    internal class NotEmpty : Constraint
    {
        /// <inheritdoc/>
        protected override Type InputType => typeof(IEnumerable);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (!((IEnumerable)value).Any())
            {
                yield return $"{name} cannot be empty";
            }
        }
    }
}