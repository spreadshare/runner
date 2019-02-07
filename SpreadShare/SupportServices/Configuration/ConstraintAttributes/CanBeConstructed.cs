using System;
using System.Data;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain string can be used on the .ctor(string) of a certain type.
    /// </summary>
    internal class CanBeConstructed : Constraint
    {
        private readonly Type _target;
        private string _failureCache;

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
        public override string OnError(string name, object value)
            => $"{name} has value '{value}' which was rejected by the .ctor(string) method of {_target}: {_failureCache}";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            try
            {
                return Activator.CreateInstance(_target, value) != null;
            }
            catch (Exception e)
            {
                _failureCache = e.Message;
                return false;
            }
        }
    }
}