using System;
using System.Data;
using System.Reflection;
using Dawn;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a string can be parsed by the 'static T Parse(string)' method of given type T.
    /// </summary>
    internal sealed class ParsesToClass : Constraint
    {
        private readonly Type _target;
        private readonly MethodInfo _parseMethod;
        private string _failureCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsesToClass"/> class.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public ParsesToClass(Type type)
        {
            _target = type;
            _parseMethod = _target.GetMethod("Parse", new[] { InputType });
            Guard.Argument(type)
                .Require<InvalidConstraintException>(
                    x => x.IsClass,
                    x => $"{x.Name} is not a class and therefore not a valid argument for ParsesToClass")
                .Require<InvalidConstraintException>(
                    _ => _parseMethod != null,
                    x => $"{x.Name} does not contain the 'Parse(string)' method, therefore the ParsesToClass Attribute is invalid");
        }

        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} has the value '{value}' which was not successfully parsed by the Parse(string) method of {_target.Name}. {_failureCache}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            try
            {
                return _parseMethod.Invoke(null, new[] { value }) != null;
            }
            catch (Exception e)
            {
                _failureCache = e.InnerException?.Message;
                return false;
            }
        }
    }
}