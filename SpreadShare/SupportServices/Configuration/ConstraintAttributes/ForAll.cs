using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes a given constraint on every element of an IEnumerable.
    /// </summary>
    internal class ForAll : Constraint
    {
        private readonly Constraint _implementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForAll"/> class.
        /// </summary>
        /// <param name="type">The type of constraint to use.</param>
        /// <param name="data">The arguments for the constraint.</param>
        public ForAll(Type type, params object[] data)
        {
            if (!type.IsSubclassOf(typeof(Constraint)))
            {
                throw new InvalidConstraintException($"ForAll requires a type argument that is a Constraint");
            }

            _implementation = Activator.CreateInstance(type, data) as Constraint;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(IEnumerable);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            int index = 0;
            foreach (var item in (IEnumerable)value)
            {
                if (!_implementation.Valid(item))
                {
                    foreach (var failure in _implementation.Validate($"{name}[{index}]", item))
                    {
                        yield return failure;
                    }
                }

                index++;
            }
        }
    }
}