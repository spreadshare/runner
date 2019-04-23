using System;
using System.Collections.Generic;
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
        private readonly MethodInfo _parseMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsesToClass"/> class.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public ParsesToClass(Type type)
        {
            _parseMethod = type.GetMethod("Parse", new[] { InputType });
            Guard.Argument(type)
                .Require<InvalidConstraintException>(
                    x => x.IsClass,
                    x => $"{x.Name} is not a class and therefore not a valid argument for ParsesToClass")
                .Require<InvalidConstraintException>(
                    _ => _parseMethod != null,
                    x => $"{x.Name} does not contain the 'Parse(string)' method, therefore the ParsesToClass Attribute is invalid");
        }

        /// <inheritdoc/>
        protected override Type InputType { get; } = typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            string message = null;
            try
            {
                var unused = _parseMethod.Invoke(null, new[] { value });
            }
            catch (Exception e)
            {
                message = name + " --> " + e.InnerException?.Message + $" (from method: {_parseMethod})";
            }

            if (message != null)
            {
                yield return message;
            }
        }
    }
}