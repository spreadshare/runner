using System;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace SpreadShare.SupportServices.BacktestDaemon.CommandAttributes
{
    /// <summary>
    /// Attribute that describes the arguments of a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommandArguments : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandArguments"/> class.
        /// </summary>
        /// <param name="args">args.</param>
        public CommandArguments(params string[] args)
        {
            ValueList = args;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandArguments"/> class.
        /// </summary>
        /// <param name="source">The source object of the arguments.</param>
        public CommandArguments(Type source)
        {
            Source = source;
        }

        /// <summary>
        /// Gets the values of the CommandArguments.
        /// </summary>
        public string[] Values => Source is null
            ? ValueList
            : Source.GetProperties().Select(x =>
            {
                var attribute = x.GetCustomAttribute<OptionAttribute>();
                return attribute.LongName + (attribute.Required ? "*" : string.Empty);
            }).ToArray();

        /// <summary>
        /// Gets the value of the CommandArguments.
        /// </summary>
        private string[] ValueList { get; }

        /// <summary>
        /// Gets the source type of the CommandArguments.
        /// </summary>
        private Type Source { get; }
    }
}