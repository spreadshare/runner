using System;
using System.Collections;
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
        public override string OnError(string name, object value) => $"{name} cannot be empty";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            return ((IEnumerable)value).Any();
        }
    }
}