using System;
using Newtonsoft.Json;
using SpreadShare.Models.Trading;

namespace SpreadShare.Serializers
{
    /// <summary>
    /// Specifies JSON reading and writing routines that facilitate serialization and deserialization.
    /// </summary>
    internal class CurrencySerializer
        : JsonConverter
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