using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using SpreadShare.Utilities;
using YamlDotNet.Serialization;

// SA1201: Disable fields before properties: all yaml members at the top
// SA1300: Disable naming conventions (parsable properties must be public)
// SA1600: Elements must be documented, public yaml members ought not te be used.
#pragma warning disable SA1300, SA1201, SA1600

namespace SpreadShare.SupportServices.Configuration
{
    /// <summary>
    /// Configuration for an algorithm, with all write able properties.
    /// </summary>
    internal class AlgorithmConfiguration
    {
        // ###      YAML MEMBERS      ###
        [YamlMember(Alias = "Exchange")]
        [Required]
        [ParsesToEnum(typeof(Exchange))]
        public string __exchange { get; protected set; }

        [YamlMember(Alias = "TradingPairs")]
        [Required]
        [NotEmpty]
        [ForAll(typeof(ParsesToClass), typeof(TradingPair))]
        public List<string> __tradingPairs { get; protected set; }

        // ###    PRIVATE PARSERS    ###
        private readonly LazyCache<string, Exchange> _exchangeConstructor = new LazyCache<string, Exchange>(Enum.Parse<Exchange>);
        private readonly LazyCache<List<string>, List<TradingPair>> _activeTradingPairsConstructor = new LazyCache<List<string>, List<TradingPair>>(x => x.Select(TradingPair.Parse).ToList());
        private readonly LazyCache<List<TradingPair>, Currency> _baseCurrencyConstructor = new LazyCache<List<TradingPair>, Currency>(
            x =>
            {
                if (x.TrueForAll(y => y.Right == x[0].Right))
                {
                    return x[0].Right;
                }

                throw new InvalidConfigurationException("Not all TradingPairs have the same base currency.");
            });

        // ###    PUBLIC PARSED PROPERTIES ###

        /// <summary>
        /// Gets the exchange at which the algorithm operates.
        /// </summary>
        public Exchange Exchange => _exchangeConstructor.Value(__exchange);

        /// <summary>
        /// Gets th base currency of the algorithm.
        /// </summary>
        [Required]
        public Currency BaseCurrency => _baseCurrencyConstructor.Value(TradingPairs);

        /// <summary>
        /// Gets the trading pairs of the algorithm.
        /// </summary>
        public List<TradingPair> TradingPairs => _activeTradingPairsConstructor.Value(__tradingPairs);
    }
}

#pragma warning restore SA1300, SA1600
