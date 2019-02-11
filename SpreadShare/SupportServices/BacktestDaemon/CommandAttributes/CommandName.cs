using System;

namespace SpreadShare.SupportServices.BacktestDaemon.CommandAttributes
{
    /// <summary>
    /// Attribute that tells you the command string associated with it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal class CommandName : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandName"/> class.
        /// </summary>
        /// <param name="name">name of the command.</param>
        public CommandName(string name)
        {
            Value = name;
        }

        /// <summary>
        /// Gets the value of the CommandName.
        /// </summary>
        public string Value { get; }
    }
}