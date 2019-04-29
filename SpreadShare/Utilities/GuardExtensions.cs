using System;
using Dawn;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Extension methods for the Guard library.
    /// </summary>
    internal static class GuardExtensions
    {
        /// <summary>
        /// Checks whether an argument is within an inclusive range.
        /// </summary>
        /// <param name="argumentInfo">The argument to check.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <param name="message">Optional message generator.</param>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <returns>The initial <paramref name="argumentInfo"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the value of the argumentInfo was outside the provided range.</exception>
        public static Guard.ArgumentInfo<T> InRangeInclusive<T>(
            this Guard.ArgumentInfo<T> argumentInfo,
            decimal min,
            decimal max,
            Func<T, string> message = null)
            where T : IComparable
        {
            if (!argumentInfo.HasValue())
            {
                throw new ArgumentOutOfRangeException($"Cannot check, provided {nameof(argumentInfo)} has no value..");
            }

            if (argumentInfo.Value.CompareTo(min) >= 0 && argumentInfo.Value.CompareTo(max) <= 0)
            {
                return argumentInfo;
            }

            if (message == null)
            {
                throw new ArgumentOutOfRangeException($"{argumentInfo.Name} was not in range [{min}, {max}]");
            }

            throw new ArgumentOutOfRangeException(message(argumentInfo.Value));
        }

        /// <summary>
        /// Check whether a type is an implementation of IBaseAlgorithm.
        /// </summary>
        /// <param name="argumentInfo">The argument to check.</param>
        /// <param name="message">Optional message generator.</param>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <returns>The initial <paramref name="argumentInfo"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided type is not an Algorithm.</exception>
        public static Guard.ArgumentInfo<T> IsAlgorithm<T>(
            this Guard.ArgumentInfo<T> argumentInfo,
            Func<T, string> message = null)
            where T : Type
        {
            if (!argumentInfo.HasValue())
            {
                throw new ArgumentOutOfRangeException($"Cannot check, provided {nameof(argumentInfo)} has no value.");
            }

            if (Reflections.IsAlgorithm(argumentInfo.Value))
            {
                return argumentInfo;
            }

            if (message == null)
            {
                throw new ArgumentOutOfRangeException($"{argumentInfo.Value} is not an algorithm");
            }

            throw new ArgumentOutOfRangeException(message(argumentInfo.Value));
        }

        /// <summary>
        /// Check whether a type is an implementation of AlgorithmConfiguration.
        /// </summary>
        /// <param name="argumentInfo">The argument to check.</param>
        /// <param name="message">Optional message generator.</param>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <returns>The initial <paramref name="argumentInfo"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided type is not an AlgorithmConfiguration.</exception>
        public static Guard.ArgumentInfo<T> IsAlgorithmConfiguration<T>(
            this Guard.ArgumentInfo<T> argumentInfo,
            Func<T, string> message = null)
            where T : Type
        {
            if (!argumentInfo.HasValue())
            {
                throw new ArgumentOutOfRangeException($"Cannot check, provided {nameof(argumentInfo)} has no value.");
            }

            if (Reflections.IsAlgorithmConfiguration(argumentInfo.Value))
            {
                return argumentInfo;
            }

            if (message == null)
            {
                throw new ArgumentOutOfRangeException($"{argumentInfo.Value} is not an AlgorithmConfiguration.");
            }

            throw new ArgumentOutOfRangeException(message(argumentInfo.Value));
        }
}
}