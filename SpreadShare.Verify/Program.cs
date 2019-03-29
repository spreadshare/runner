using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using YamlDotNet.Serialization;

namespace SpreadShare.Verify
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                var versionString = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

                Console.WriteLine($"spreadshare verify v{versionString}");
                Console.WriteLine("-------------");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  dotnet verify <file>");
                return 2;
            }

            var filename = args[0];
            StreamReader source;
            try
            {
                source = new StreamReader(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            // Ensure trading pairs can be verified.
            TradingPair.Sync();

            var config = new DeserializerBuilder().Build().Deserialize<Configuration>(source);
            var failures = ConfigurationValidator.GetConstraintFailuresRecursively(config).ToArray();
            if (!failures.Any())
            {
                return 0;
            }

            foreach (var failure in failures)
            {
                Console.WriteLine(failure);
            }

            return 0;
        }
    }
}