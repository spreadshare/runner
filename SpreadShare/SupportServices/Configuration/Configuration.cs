using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawn;
using SpreadShare.Algorithms;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using SpreadShare.Utilities;
using YamlDotNet.Serialization;

// SA1300: Disable naming conventions, YamlMembers must be public but should not be used.
// SA1402: Allow all nested settings classes in one file.
// SA1600: Disable comment checking to maintain readable.
#pragma warning disable SA1300, SA1402, SA1600

namespace SpreadShare.SupportServices.Configuration
{
    internal class Configuration
    {
        public static Configuration Instance;

        [Required]
        public ConnectionStrings ConnectionStrings { get; private set; }

        public LoggingSettings LoggingSettings { get; private set; }

        [Required]
        public BinanceClientSettings BinanceClientSettings { get; private set; }

        [Required]
        public BacktestSettings BacktestSettings { get; private set; }

        public EnabledAlgorithm EnabledAlgorithm { get; private set; }

        public int CandleWidth { get; } = 5;

        /// <summary>
        /// Lift the current instance to the static instance.
        /// </summary>
        public void Bind() => Instance = this;
    }

    /// <summary>
    /// A collection of ConnectionStrings.
    /// </summary>
    internal class ConnectionStrings
    {
        /// <summary>
        /// Gets the LocalConnection information for database connections.
        /// </summary>
        [Required]
        [YamlMember(SerializeAs = typeof(object))] // redacted
        public string LocalConnection { get; private set; }
    }

    internal class LoggingSettings
    {
        [YamlMember(SerializeAs = typeof(object))] // redacted
        public string SentryDSN { get; private set; }
    }

    internal class BinanceClientSettings
    {
        [RangeLong(5000, 20000)]
        public long ReceiveWindow { get; private set; }

        /// <summary>
        /// Gets the maximum number of candles fetched per request.
        /// </summary>
        [RangeInt(100, 900)]
        public int CandleRequestSize { get; private set; }

        [Required]
        public CredentialsWrapper Credentials { get; private set; }

        public class CredentialsWrapper
        {
            [Required]
            [YamlMember(SerializeAs = typeof(object))] // redacted
            public string Key { get; private set; }

            [Required]
            [YamlMember(SerializeAs = typeof(object))] // redacted
            public string Secret { get; private set; }
        }
    }

    internal class BacktestSettings
    {
        [Required]
        public string OutputFolder { get; private set; }
    }

    internal class EnabledAlgorithm
    {
        private readonly LazyCache<string, Type> _enabledAlgorithmConstructor =
            new LazyCache<string, Type>(x => Reflections.AllAlgorithms.First(a => a.Name == x));

        private readonly LazyCache<Dictionary<string, decimal>, Portfolio> _allocationConstructor =
            new LazyCache<Dictionary<string, decimal>, Portfolio>(
                x => new Portfolio(x.ToDictionary(
                    y => new Currency(y.Key),
                    y => new Balance(new Currency(y.Key), y.Value, 0))));

        private readonly LazyCache<Dictionary<string, object>> _algorithmConfigurationConstructor =
            new LazyCache<Dictionary<string, object>>();

        private Type _algorithmConfigurationType;

        [Required]
        [YamlMember(Alias = "Algorithm")]
        [IsImplementation(typeof(IBaseAlgorithm))]
        public string __algorithm { get; private set; }

        [YamlMember(Alias = "Parameters")]
        public Dictionary<string, object> __parameters { get; private set; }

        [YamlMember(Alias = "Allocation")]
        [Required]
        [NotEmpty]
        [ForKeys(typeof(CanBeConstructed), typeof(Currency))]
        [ForValues(typeof(RangeDecimal), "0", "79228162514264337593543950335")]
        public Dictionary<string, decimal> __allocation { get; private set; }

        public Type Algorithm => _enabledAlgorithmConstructor.Value(__algorithm);

        public Portfolio Allocation => _allocationConstructor.Value(__allocation).Clone();

        [ForceEval]
        public AlgorithmConfiguration AlgorithmConfiguration
        {
            get
            {
                _algorithmConfigurationType = Reflections.GetMatchingConfigurationsType(Algorithm);

                try
                {
                    return _algorithmConfigurationConstructor.Value(
                        __parameters,
                        x => new DeserializerBuilder().Build().Deserialize( // Re-Deserialize with the now known type.
                                new SerializerBuilder().Build().Serialize(__parameters),
                                _algorithmConfigurationType),
                        _algorithmConfigurationType) as AlgorithmConfiguration;
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            private set
            {
                __parameters = new Dictionary<string, object>();
                foreach (var property in value.GetType().GetProperties().Where(x => x.CanWrite).Where(x => x.GetCustomAttribute<YamlIgnoreAttribute>() == null))
                {
                    var name = property.Name;
                    var member = property.GetCustomAttribute<YamlMemberAttribute>();
                    if (member != null)
                    {
                        name = member.Alias;
                    }

                    var val = property.GetValue(value);
                    __parameters[name] = val;
                    if (val is IEnumerable list)
                    {
                        Console.WriteLine($"Set {name} to {string.Join(", ", list.Cast<string>().ToArray())}");
                    }
                    else
                    {
                        Console.WriteLine($"Set {name} to {property.GetValue(value)}");
                    }
                }

                _algorithmConfigurationConstructor.Invalidate();
            }
        }

        public void ChangeAlgorithmConfiguration(Type algorithm, AlgorithmConfiguration config)
        {
            Guard.Argument(algorithm).Require(Reflections.IsAlgorithm, x => $"{x} is not an algorithm");
            if (!Reflections.AlgorithmMatchesConfiguration(algorithm, config.GetType()))
            {
                throw new InvalidOperationException($"Cannot set algorithm to {algorithm} with config {config} because they are not compatible.");
            }

            __algorithm = algorithm.Name;
            _enabledAlgorithmConstructor.Invalidate();
            AlgorithmConfiguration = config;
        }
    }
}

#pragma warning restore SA1300, SA1402, SA1600
