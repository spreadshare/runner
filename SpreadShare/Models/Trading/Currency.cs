using System;
using Dawn;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a currency.
    /// </summary>
    [JsonConverter(typeof(CurrencySerializer))]
    internal class Currency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of a currency.</param>
        public Currency(string symbol)
        {
            Guard.Argument(symbol).NotNull().NotEmpty().NotWhiteSpace();
            Symbol = symbol.ToUpperInvariant().Trim();
        }

        /// <summary>
        /// Gets the symbol of a currency
        /// TODO: Should this not be named ticker?.
        /// </summary>
        public string Symbol { get; }

        public static bool operator ==(Currency a, Currency b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        public static bool operator !=(Currency a, Currency b)
        {
            if (a is null && b is null)
            {
                return false;
            }

            if (a is null || b is null)
            {
                return true;
            }

            return !a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// String representation of a currency.
        /// </summary>
        /// <returns>Returns the string representation of a currency.</returns>
        public override string ToString() => Symbol;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Currency currency && Symbol == currency.Symbol;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Symbol);
        }
    }

    /// <summary>
    /// Specifies JSON reading and writing routines that facilitate serialization and deserialization.
    /// </summary>
    internal class CurrencySerializer : JsonConverter
    {
        /// <summary>
        /// Specifies how the write the Currency object as JSON.
        /// </summary>
        /// <param name="writer">Json writer.</param>
        /// <param name="value">The currency object.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Write currency just as its symbol string
            var c = value as Currency ?? new Currency("Undefined");
            serializer.Serialize(writer, c.Symbol);
        }

        /// <summary>
        /// Specifies how to read the JSON as Currency object.
        /// </summary>
        /// <param name="reader">The json reader.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>Parsed, deserialized object.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Return new Currency instance using the string read
            string str = serializer.Deserialize<string>(reader);
            return new Currency(str);
        }

        /// <summary>
        /// Specifies whether a certain type can be converted.
        /// </summary>
        /// <param name="objectType">The type to test.</param>
        /// <returns>Bool indicating conversion capabilities.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Currency);
        }
    }
}

#pragma warning restore SA1402
