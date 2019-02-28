using System;
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
        private Type _parent;

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
        public override string OnError(string name, object value)
            => $"{name} has the value '{value}', which does not implement the interface {_parent.Name}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => Reflections.AllAlgorithms.Any(x => x.Name == (string)value);
    }
}