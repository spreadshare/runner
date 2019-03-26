using System;
using System.Collections;
using System.Collections.Generic;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Template constraint that provider utility methods for handling dictionaries.
    /// </summary>
    internal abstract class DictionaryConstraint : Constraint
    {
        /// <inheritdoc/>
        protected sealed override Type InputType => typeof(IDictionary);

        /// <summary>
        /// Cast a dictionary to an enumerable dictionary entry instance.
        /// </summary>
        /// <param name="dictionary">The dictionary to cast.</param>
        /// <returns>Enumerable Dictionary entries.</returns>
        protected static IEnumerable<DictionaryEntry> CastDict(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }
    }
}