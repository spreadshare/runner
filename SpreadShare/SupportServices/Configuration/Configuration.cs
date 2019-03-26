using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
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

        private readonly LazyCache<string, CandleWidth> _candleWidthConstructor =
            new LazyCache<string, CandleWidth>(Enum.Parse<CandleWidth>);

        [YamlMember(Alias = "CandleWidth")]
        [Required]
        [ParsesToEnum(typeof(CandleWidth))]
        public string __candleWidth { get; private set; }

        [Required]
        public ConnectionStrings ConnectionStrings { get; private set; }

        public LoggingSettings LoggingSettings { get; private set; }

        [Required]
        public BinanceClientSettings BinanceClientSettings { get; private set; }

        [Required]
        public BacktestSettings BacktestSettings { get; private set; }

        public EnabledAlgorithm EnabledAlgorithm { get; private set; }

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
        [ForKeys(typeof(CanBeConstructed), typeof(Currency))]
        [ForValues(typeof(RangeDecimal), "0.0", "79228162514264337593543950335")]
        public Dictionary<string, decimal> __portfolio { get; private set; }

        public Portfolio Portfolio => _portfolioConstructor.Value(__portfolio).Copy();
    }

    internal class EnabledAlgorithm
    {
        private readonly LazyCache<string, Type> _enabledAlgorithmConstructor =
            new LazyCache<string, Type>(x => Reflections.AllAlgorithms.First(a => a.Name == x));

        private readonly LazyCache<string, Exchange> _exchangeConstructor =
            new LazyCache<string, Exchange>(Enum.Parse<Exchange>);

        private readonly LazyCache<Dictionary<string, decimal>, Portfolio> _allocationConstructor =
            new LazyCache<Dictionary<string, decimal>, Portfolio>(
                x => new Portfolio(x.ToDictionary(
                    y => new Currency(y.Key),
                    y => new Balance(new Currency(y.Key), y.Value, 0))));

        public Type Algorithm => _enabledAlgorithmConstructor.Value(__algorithm);

        [Required]
        [YamlMember(Alias = "Algorithm")]
        [IsImplementation(typeof(IBaseAlgorithm))]
        public string __algorithm { get; private set; }

        [YamlMember(Alias = "Exchange")]
        [Required]
        [ParsesToEnum(typeof(Exchange))]
        public string __exchange { get; private set; }

        [YamlMember(Alias = "Allocation")]
        [Required]
        [NotEmpty]
        [ForKeys(typeof(CanBeConstructed), typeof(Currency))]
        [ForValues(typeof(RangeDecimal), "0", "79228162514264337593543950335")]
        public Dictionary<string, decimal> __allocation { get; private set; }

        public Exchange Exchange => _exchangeConstructor.Value(__exchange);

        public Portfolio Allocation => _allocationConstructor.Value(__allocation);
    }
}

#pragma warning restore SA1300, SA1402, SA1600
