using System;

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
        public ParsesToEnum(Type type) => _target = type;

        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} has value '{value}' which is not a member of the {_target.Name} enum.";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
            => Enum.TryParse(_target, (string)value, out var unused);
    }
}