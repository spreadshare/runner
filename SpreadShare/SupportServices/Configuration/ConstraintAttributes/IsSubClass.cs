using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using SpreadShare.Utilities;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain string is the name of a type that is a certain subclass of the given type.
    /// </summary>
    internal class IsSubClass : Constraint
    {
        private readonly Type _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsSubClass"/> class.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public IsSubClass(Type type)
        {
            Guard.Argument(type)
                .Require(
                    x => x.IsClass,
                    x => $"{x.Name} is not a class, did you mean to invoke IsImplementation?");
            _parent = type;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (!Reflections.GetAllSubtypes(_parent).Select(x => x.Name).Contains((string)value))
            {
                yield return $"{name} has value {value} which is not a subclass of {_parent.Name}";
            }
        }
    }
}