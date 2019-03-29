using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using SpreadShare.Utilities;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that certain string maps to type which is has the given type as interface.
    /// </summary>
    internal class IsImplementation : Constraint
    {
        private readonly Type _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsImplementation"/> class.
        /// </summary>
        /// <param name="type">Type to check.</param>
        public IsImplementation(Type type)
        {
            Guard.Argument(type)
                .Require(
                    x => x.IsInterface,
                    x => $"{x.Name} is not an interface, did you mean to invoke IsSubClass?");
            _parent = type;
        }

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (Reflections.AllAlgorithms.All(x => x.Name != (string)value))
            {
                yield return $"{name} has the value '{value}', which does not implement the interface {_parent.Name}";
            }
        }
    }
}