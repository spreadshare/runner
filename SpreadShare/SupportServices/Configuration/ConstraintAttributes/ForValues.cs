using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes a given constraint on every element of an IEnumerable.
    /// </summary>
    internal class ForValues : DictionaryConstraint
    {
        private readonly Constraint _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForValues"/> class.
        /// </summary>
        /// <param name="type">The type of constraint to use.</param>
        /// <param name="data">The arguments for the constraint.</param>
        public ForValues(Type type, params object[] data)
        {
            if (!type.IsSubclassOf(typeof(Constraint)))
            {
                throw new InvalidConstraintException($"ForAll requires a type argument that is a Constraint");
            }

            _implementation = Activator.CreateInstance(type, data) as Constraint;
        }

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            int index = 0;
            foreach (var key in CastDict((IDictionary)value).Select(x => x.Value))
            {
                foreach (var failure in _implementation.Validate($"{name}.Values[{index}]", key))
                {
                    yield return failure;
                }

                index++;
            }
        }
    }
}
