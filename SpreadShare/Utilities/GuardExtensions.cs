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
        public static Guard.ArgumentInfo<T> InRangeInclusive<T>(this Guard.ArgumentInfo<T> argumentInfo, decimal min, decimal max, Func<T, string> message = null)
            where T : IComparable
        {
            if (!argumentInfo.HasValue())
            {
                throw new ArgumentOutOfRangeException($"Cannot check range if the provided {nameof(argumentInfo)} has no value..");
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
    }
}