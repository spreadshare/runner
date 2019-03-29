using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
        /// Generator method that gives a description of all constraint violations.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>A list of error message.</returns>
        /// <exception cref="InvalidConstraintException">If the value has an invalid type given the constraint.</exception>
        public IEnumerable<string> Validate(string name, object value)
        {
            var castMethod = GetType().GetMethod("Cast").MakeGenericMethod(InputType);
            object castable = castMethod.Invoke(this, new[] { value });
            if (!(bool)castable && value != null)
            {
               throw new InvalidConstraintException($"{GetType().Name} constraint can only be used on values that are {InputType.Name}");
            }

            return GetErrors(name, value);
        }

        /// <summary>
        /// Returns a value indicating whether the constraint holds.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether the constraint holds for <paramref name="value"/>.</returns>
        public bool Valid(object value) => Validate(string.Empty, value).Any();

        /// <summary>
        /// Tells whether an object is of a certain type.
        /// </summary>
        /// <param name="o">The object to evaluate.</param>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>Whether o is T.</returns>
        public bool Cast<T>(object o) => o is T;

        /// <summary>
        /// Returns all the violations generated by the constraint.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>List of violations.</returns>
        protected abstract IEnumerable<string> GetErrors(string name, object value);
    }
}