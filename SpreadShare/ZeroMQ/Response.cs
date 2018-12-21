using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpreadShare.ZeroMQ
{
    /// <summary>
    /// Response for a received ZeroMQ command.
    /// </summary>
    internal class Response
    {
        /// <summary>
        /// Result of the action.
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public Type Result;

        /// <summary>
        /// Message concerning the result.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class.
        /// </summary>
        /// <param name="type">Result of the action.</param>
        /// <param name="message">Message concerning the result.</param>
        public Response(Type type, string message)
        {
            Result = type;
            Message = message;
        }

        /// <summary>
        /// Result type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        internal enum Type
        {
            /// <summary>
            /// Action resulted in an error
            /// </summary>
            [EnumMember(Value = "error")]
            Error,

            /// <summary>
            /// Action is invalid in its current context
            /// </summary>
            [EnumMember(Value = "failure")]
            Failure,

            /// <summary>
            /// Action executed successfully
            /// </summary>
            [EnumMember(Value = "success")]
            Success,
        }

        /// <summary>
        /// Serializes the response to JSON.
        /// </summary>
        /// <returns>A JSON serialized response.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
