using System;
using System.Data;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes a constraint on a certain attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    internal abstract class Constraint : Attribute
    {
        /// <summary>
        /// Gets type that input of the predicate has to inherit from.
        /// </summary>
        protected virtual Type InputType => typeof(object);

        /// <summary>
        /// Error message that explains why the predicate does not hold.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>Error message.</returns>
        public abstract string OnError(string name, object value);

        /// <summary>
        /// Check if a value is valid.
        /// </summary>
        /// <param name="value">The value of the property.</param>
        /// <returns>Truth value regarding the validity.</returns>
        public bool Valid(object value)
        {
            var castMethod = GetType().GetMethod("Cast").MakeGenericMethod(InputType);
            object castable = castMethod.Invoke(this, new[] { value });
            if (!(bool)castable && value != null)
            {
               throw new InvalidConstraintException($"{GetType().Name} constraint can only be used on values that are {InputType.Name}");
            }

            return Predicate(value);
        }

        /// <summary>
        /// Tells whether an object is of a certain type.
        /// </summary>
        /// <param name="o">The object to evaluate.</param>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>Whether o is T.</returns>
        public bool Cast<T>(object o) => o is T;

        /// <summary>
        /// Implements the actual predicate for the constraint.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>Whether the predicate holds.</returns>
        protected abstract bool Predicate(object value);
    }
}