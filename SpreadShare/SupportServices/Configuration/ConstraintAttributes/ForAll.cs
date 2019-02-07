using System;
using System.Collections;
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
        /// Keeps track of the last failed element to enrich the error message.
        /// </summary>
        private (object, int) _failureCache;

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
        public override string OnError(string name, object value)
            => $"ForAll failure: {_implementation.OnError($"{name}[{_failureCache.Item2}]", _failureCache.Item1)}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            int index = 0;
            foreach (var item in (IEnumerable)value)
            {
                if (!_implementation.Valid(item))
                {
                    _failureCache = (item, index);
                    return false;
                }

                index++;
            }

            return true;
        }
    }
}