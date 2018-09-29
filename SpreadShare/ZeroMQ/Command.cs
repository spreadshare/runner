using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpreadShare.ZeroMQ
{
    internal abstract class Command
    {
        protected ILogger _logger;
        protected abstract string CommandString { get; }
        protected virtual Dictionary<string, string> Arguments => new Dictionary<string, string>();

        public static Command GetCommand(string json, ILoggerFactory loggerFactory)
        {
            // ParseArguments json
            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(json);
            }
            catch (JsonReaderException e)
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
                var commandString = type.GetProperty("CommandString", 
                    BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(subcommand);

                // Check CommandString with json
                if (jsonObject["command"].ToString().Equals(commandString))
                {
                    var x = (Command) subcommand;
                    x._logger = loggerFactory.CreateLogger(type);
                    x.ParseArguments(jsonObject);
                    return x;
                }
            }
            throw new Exception($"Command '{jsonObject["command"]}' was not recognised.");
        }
        
        /// <summary>
        /// Parse arguments required
        /// </summary>
        /// <param name="jsonObject"></param>
        private void ParseArguments(JObject jsonObject)
        {
            // No arguments
            if (Arguments.Count <= 0) return;

            // Check if each key exists in jsonObject and assign
            foreach (var arg in Arguments)
            {
                try
                {
                    Arguments[arg.Key] = jsonObject[arg.Key].ToString();
                }
                catch (Exception e)
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
        public abstract void Action();
    }
}
