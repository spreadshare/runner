using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;
using SpreadShare.SupportServices.BacktestDaemon.Commands;
using SpreadShare.Utilities;

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
        public BacktestDaemonService(AlgorithmService algoService, AllocationManager allocationManager)
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