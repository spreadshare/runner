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
        protected abstract string[] RequiredArguments { get; }

        /* TODO: 
         * Create hideous switch case for each command
         * Create subclass for each command with action implementation
         */

        public static Command GetCommand(string json)
        {
            dynamic jsonObject = JObject.Parse(json);
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Command))
                select type;

            foreach (var type in subclasses)
            {
                object subcommand = Activator.CreateInstance(type);
                var commandString = type.GetProperty("CommandString", 
                    BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(subcommand);
                if (jsonObject.command == commandString)
                {
                    return (Command)subcommand;
                }
            }
            return null;
        }

        public abstract void Parse();
        public abstract void Action();
    }
}
