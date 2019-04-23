using System;
using System.Linq;
using System.Reflection;
using Dawn;
using Microsoft.EntityFrameworkCore.Internal;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;
using SpreadShare.SupportServices.BacktestDaemon.Commands;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Utilities;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SpreadShare.SupportServices.BacktestDaemon
{
    /// <summary>
    /// Command line interface for backtesting.
    /// </summary>
    internal class BacktestDaemonService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDaemonService"/> class.
        /// </summary>
        /// <param name="algoService">The algorithm service.</param>
        /// <param name="allocationManager">The allocation manager.</param>
        public BacktestDaemonService(AlgorithmService algoService, IAllocationManager allocationManager)
        {
            State = new BacktestDaemonState(algoService, allocationManager);
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static BacktestDaemonService Instance { get; private set; }

        /// <summary>
        /// Gets the state of the daemon service.
        /// </summary>
        public BacktestDaemonState State { get; private set; }

        /// <summary>
        /// Gets an AlgorithmConfiguration of a certain type from the user over the command line.
        /// </summary>
        /// <param name="configuration">The type of configuration.</param>
        /// <returns>Parsed AlgorithmConfiguration.</returns>
        public static AlgorithmConfiguration GetConfigurationFromUser(Type configuration)
        {
            Guard.Argument(configuration).IsAlgorithmConfiguration();
            var properties = configuration
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.CanWrite)
                .ToArray();
            var propertyNames = properties.Select(x => x.GetCustomAttribute<YamlMemberAttribute>()?.Alias ?? x.Name);
            var config = (AlgorithmConfiguration)Activator.CreateInstance(configuration);
            foreach (var (name, property) in propertyNames.Zip(properties, (a, b) => (a, b)))
            {
                object parsed = null;
                while (parsed == null)
                {
                    Console.Write(name + ": ");
                    var val = Console.ReadLine();
                    try
                    {
                        parsed = new DeserializerBuilder().Build().Deserialize(val, property.PropertyType);
                    }
                    catch (YamlException)
                    {
                        Console.WriteLine($"ERROR: Could not parse input, input must be yaml parsable to {property.PropertyType.Name}");
                        continue;
                    }

                    var failures = ConfigurationValidator.GetPropertyFailures(property, parsed).ToArray();
                    if (!failures.Any())
                    {
                        continue;
                    }

                    parsed = null; // Ensure the while loop continues.
                    Console.WriteLine("Not all constraints hold:");
                    Console.WriteLine(string.Join("\n", failures));
                }

                property.SetValue(config, parsed);
            }

            return config;
        }

        /// <summary>
        /// Lift the current instance to the static singleton.
        /// </summary>
        public void Bind()
        {
            Instance = this;
        }

        /// <summary>
        /// Start the daemon.
        /// </summary>
        public void Run()
        {
             Console.WriteLine(FormatTitle("Welcome to the backtest CLI"));
             while (true)
             {
                 string input = Console.ReadLine();
                 if (string.IsNullOrEmpty(input))
                 {
                     continue;
                 }

                 try
                 {
                     var command = Parse(input);
                     command.Execute(State);
                 }
                 catch (InvalidCommandException e)
                 {
                     Console.WriteLine($"COMMAND_ERROR: {e.Message}");
                 }
                 catch (Exception e)
                 {
                     Console.WriteLine($"COMMAND_ERROR: Command caused error: {e}");
                 }
             }
        }

        private static string FormatTitle(string title)
        {
            int length = title.Length + 4;
            string line = new string('-', length);
            return new[] { line, $"| {title} |", line }.Join("\n");
        }

        private static BacktestCommand Parse(string input)
        {
            string[] inputs = input.Split(' ');
            if (inputs.Length == 0)
            {
               throw new InvalidCommandException("Invalid command, usage: {command} [arguments]");
            }

            var commands = Reflections.GetAllSubtypes(typeof(BacktestCommand));
            Type commandType;
            try
            {
                commandType = commands
                    .First(x => x.GetCustomAttributes(false)
                        .OfType<CommandName>()
                        .Select(a => a.Value)
                        .Contains(inputs[0]));
            }
            catch
            {
                throw new InvalidCommandException($"{inputs[0]} is not a valid command");
            }

            try
            {
                return Activator.CreateInstance(commandType, new object[] { inputs }) as BacktestCommand;
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }
    }
}