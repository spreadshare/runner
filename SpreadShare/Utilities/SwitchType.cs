using System;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Extension method to allow for switching on the type of the object.
    /// </summary>
    internal static class SwitchType
    {
        /// <summary>
        /// Switch on the current object using a number of cases.
        /// </summary>
        /// <param name="item">The item whose type to check.</param>
        /// <param name="switchCases">The cases to consider.</param>
        public static void Switch(this object item, params SwitchCase[] switchCases)
        {
            foreach (var switchCase in switchCases)
            {
                if (switchCase.IsDefault || switchCase.Eval(item))
                {
                    switchCase.Action();
                    return;
                }
            }
        }

        /// <summary>
        /// Defines a case to execute for given target type.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <typeparam name="T">The type to target.</typeparam>
        /// <returns>A switch case instance.</returns>
        public static SwitchCase Case<T>(Action action)
            => new SwitchCase(action, typeof(T));

        /// <summary>
        /// Defines a default fallback case.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A switch case instance.</returns>
        public static SwitchCase Default(Action action)
            => new SwitchCase(action);

        /// <summary>
        /// Defines a switch case for a typed switch.
        /// </summary>
        internal class SwitchCase
        {
            private readonly Type _target;

            /// <summary>
            /// Initializes a new instance of the <see cref="SwitchCase"/> class.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            /// <param name="target">The type to target.</param>
            public SwitchCase(Action action, Type target)
            {
                Action = action;
                _target = target;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SwitchCase"/> class.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            public SwitchCase(Action action)
            {
                Action = action;
                IsDefault = true;
            }

            /// <summary>
            /// Gets a value indicating whether this case is a default case.
            /// </summary>
            public bool IsDefault { get; }

            /// <summary>
            /// Gets the action set for this case.
            /// </summary>
            public Action Action { get; }

            /// <summary>
            /// The evaluation function to check if the item matches the target type.
            /// </summary>
            /// <param name="item">Item to check.</param>
            /// <returns>Whether the action was executed.</returns>
            public bool Eval(object item) => item.GetType() == _target || item.GetType().IsSubclassOf(_target);
        }
    }
}