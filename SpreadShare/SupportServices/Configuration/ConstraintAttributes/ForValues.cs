using System;
using System.Collections;
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
        /// Keeps track of the last failed element to enrich the error message.
        /// </summary>
        private (object, int) _failureCache;

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
        public override string OnError(string name, object value)
            => $"ForAll failure: {_implementation.OnError($"{name}[{_failureCache.Item2}]", _failureCache.Item1)}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            int index = 0;
            foreach (var val in CastDict((IDictionary)value).Select(x => x.Value))
            {
                if (!_implementation.Valid(val))
                {
                    _failureCache = (val, index);
                    return false;
                }

                index++;
            }

            return true;
        }
    }
}
