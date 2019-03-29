using System;
using System.Collections.Generic;
using System.Data;
using Dawn;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes the value of a property to be parsable to a certain enum.
    /// </summary>
    internal class ParsesToEnum : Constraint
    {
        private readonly Type _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsesToEnum"/> class.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        public ParsesToEnum(Type type)
        {
            Guard.Argument(type)
                .Require<InvalidConstraintException>(
                    x => x.IsEnum,
                    x => $"The {nameof(ParsesToEnum)} constraint must take a Enum type argument.");
            _target = type;
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            if (!Enum.TryParse(_target, (string)value, out var unused))
            {
                yield return $"{name} has value '{value}' which is not a member of the {_target.Name} enum.";
            }
        }
    }
}