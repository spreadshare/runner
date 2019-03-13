using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly LazyCache<string, Type> _enabledAlgorithmConstructor =
            new LazyCache<string, Type>(x => Reflections.AllAlgorithms.First(a => a.Name == x));

        private readonly LazyCache<string, CandleWidth> _candleWidthConstructor =
            new LazyCache<string, CandleWidth>(Enum.Parse<CandleWidth>);

        [Required]
        public ConnectionStrings ConnectionStrings { get; private set; }

        public LoggingSettings LoggingSettings { get; private set; }

        [Required]
        public BinanceClientSettings BinanceClientSettings { get; private set; }

        [Required]
        public BacktestSettings BacktestSettings { get; private set; }

        [YamlMember(Alias = "EnabledAlgorithm")]
        [Required]
        [IsImplementation(typeof(IBaseAlgorithm))]
        public string __enabledAlgorithm { get; private set; }

        [YamlMember(Alias = "CandleWidth")]
        [Required]
        [ParsesToEnum(typeof(CandleWidth))]
        public string __candleWidth { get; private set; }

        public Type EnabledAlgorithm => _enabledAlgorithmConstructor.Value(__enabledAlgorithm);

        public CandleWidth CandleWidth => _candleWidthConstructor.Value(__candleWidth);

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
        private readonly LazyCache<Dictionary<string, decimal>, Portfolio> _portfolioConstructor =
            new LazyCache<Dictionary<string, decimal>, Portfolio>(
                x => new Portfolio(x.Select(
                        kv => (new Currency(kv.Key), // string -> currency
                        new Balance(new Currency(kv.Key), kv.Value, 0M))) // decimal -> balance
                    .ToDictionary(k => k.Item1, k => k.Item2)));

        [Required]
        public string OutputFolder { get; private set; }

        [YamlMember(Alias = "Portfolio")]
        [Required]
        [NotEmpty]
        public Dictionary<string, decimal> __portfolio { get; private set; }

        public Portfolio Portfolio => _portfolioConstructor.Value(__portfolio);
    }
}

#pragma warning restore SA1300, SA1402, SA1600