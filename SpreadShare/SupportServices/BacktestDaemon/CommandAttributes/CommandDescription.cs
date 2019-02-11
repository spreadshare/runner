using System;

namespace SpreadShare.SupportServices.BacktestDaemon.CommandAttributes
{
    /// <summary>
    /// Provides the description of a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommandDescription : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDescription"/> class.
        /// </summary>
        /// <param name="value">The description of the command.</param>
        public CommandDescription(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of the CommandDescription.
        /// </summary>
        public string Value { get; }
    }
}