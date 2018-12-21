using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpreadShare.ZeroMQ
{
    /// <summary>
    /// Base class for ZeroMQ commands.
    /// </summary>
    internal abstract class Command
    {
        /// <summary>
        /// Provides logging.
        /// </summary>
        protected ILogger Logger;

        /// <summary>
        /// Gets the identifier for the command.
        /// </summary>
        protected abstract string CommandString { get; }

        /// <summary>
        /// Gets the dictionary of required arguments with their value for the command.
        /// </summary>
        protected virtual Dictionary<string, string> Arguments => new Dictionary<string, string>();

        /// <summary>
        /// Identify the command received.
        /// </summary>
        /// <param name="json">Json containing the command (with arguments).</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger.</param>
        /// <returns>Concrete command.</returns>
        public static Command GetCommand(string json, ILoggerFactory loggerFactory)
        {
            // ParseArguments json
            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(json);
            }
            catch (JsonReaderException)
            {
                throw new Exception("Provided json is not formatted properly.");
            }

            // Check if json contains command
            if (!jsonObject.ContainsKey("command"))
            {
                throw new Exception("Provided json does not contain 'command' key.");
            }

            // Get all command classes
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Command))
                select type;

            // Assign command to type
            foreach (var type in subclasses)
            {
                // Get CommandString
                object subcommand = Activator.CreateInstance(type);
                var commandString = type.GetProperty(
                    "CommandString",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(subcommand);

                // Check CommandString with json
                if (jsonObject["command"].ToString().Equals(commandString))
                {
                    var x = (Command)subcommand;
                    x.Logger = loggerFactory.CreateLogger(type);
                    x.ParseArguments(jsonObject);
                    return x;
                }
            }

            throw new Exception($"Command '{jsonObject["command"]}' was not recognised.");
        }

        /// <summary>
        /// Requested action to be executed.
        /// </summary>
        public abstract void Action();

        /// <summary>
        /// Parse arguments required.
        /// </summary>
        /// <param name="jsonObject">JsonObject containg the arguments.</param>
        private void ParseArguments(JObject jsonObject)
        {
            // No arguments
            if (Arguments.Count <= 0)
            {
                return;
            }

            // Check if each key exists in jsonObject and assign
            foreach (var arg in Arguments)
            {
                try
                {
                    Arguments[arg.Key] = jsonObject[arg.Key].ToString();
                }
                catch (Exception)
                {
                    throw new Exception($"Argument missing: {arg.Key}.");
                }
            }

            // Check if each key of jsonObject exists in Arguments
            foreach (var providedArg in jsonObject)
            {
                if (providedArg.Key != "command" && !Arguments.ContainsKey(providedArg.Key))
                {
                    throw new Exception($"Provided argument not recognised: '{providedArg.Key}'.");
                }
            }
        }
    }
}
