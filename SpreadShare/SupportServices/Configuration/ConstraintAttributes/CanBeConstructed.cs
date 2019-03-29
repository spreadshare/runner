using System;
using System.Collections.Generic;
using System.Data;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain string can be used on the .ctor(string) of a certain type.
    /// </summary>
    internal class CanBeConstructed : Constraint
    {
        private readonly Type _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanBeConstructed"/> class.
        /// </summary>
        /// <param name="target">The type to create.</param>
        public CanBeConstructed(Type target)
        {
            _target = target;
            if (_target.GetConstructor(new[] { typeof(string) }) == null)
            {
                throw new InvalidConstraintException($"{_target.Name} does not have a public constructor with a string as argument.");
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            string message = null;
            try
            {
                if (Activator.CreateInstance(_target, value) == null)
                {
                    throw new ArgumentException($"Cannot instantiate {_target} width {value}");
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            if (message != null)
            {
                yield return message;
            }
        }
    }
}