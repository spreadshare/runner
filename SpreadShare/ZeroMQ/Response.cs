using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpreadShare.ZeroMQ
{
    internal class Response
    {
        internal enum Type
        {
            success,
            error,
            failure
        }

        [JsonConverter(typeof(StringEnumConverter)), JsonProperty(PropertyName = "result")]
        public Type Result;

        [JsonProperty(PropertyName = "message")]
        public string Message;

        public Response(Type type, string message)
        {
            Result = type;
            Message = message;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
