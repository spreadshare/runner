using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpreadShare.ZeroMQ
{
    abstract class Command
    {
        protected abstract string CommandString { get; }
        protected virtual Dictionary<string, string> Arguments => new Dictionary<string, string>();

        public static Command GetCommand(string json)
        {
            // ParseArguments json
            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(json);
            }
            catch (JsonReaderException e)
            {
                throw new Exception("Invalid json");
            }

            // Check if json contains command
            if (!jsonObject.ContainsKey("command"))
            {
                throw new Exception("Json does not contain command");
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
                if (jsonObject["command"] == commandString)
                {
                    var x = (Command)subcommand;
                    x.ParseArguments(jsonObject);
                    return x;
                }
            }
            throw new Exception("Json command not recognised");
        }
        
        /// <summary>
        /// Parse arguments
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
                    Console.WriteLine($"{arg.Key} was not found in provided json");
                    Console.WriteLine(e);
                }
            }

            // Check if each key of jsonObject exists in Arguments
            foreach (var providedArg in jsonObject)
            {
                if (providedArg.Key != "command" && !Arguments.ContainsKey(providedArg.Key))
                {
                    throw new Exception($"No args found matching {providedArg.Key}");
                }
            }
        }
        public abstract void Action();
    }
}
