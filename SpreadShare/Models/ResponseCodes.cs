namespace SpreadShare.Models
{
    public enum ResponseCodes
    {
        Error,
        NotDefined,
        Success
    }

    public class ResponseObject<T> {
        public ResponseCodes Code { get; }

        public bool Success { get {return Code == ResponseCodes.Success;}}

        public T Data { get; }

        public ResponseObject(ResponseCodes code, T data) {
            Code = code;
            Data = data;
        }

        public ResponseObject(ResponseCodes code) {
            Code = code;
        }

        public override string ToString()
        {
            return $"{Code} | msg: {Data}";
        }


    }

    public class ResponseObject : ResponseObject<string>
    {
        public ResponseObject(ResponseCodes code, string data = "") : base(code, data)
        {
        }
        public override string ToString()
        {
            return $"{Code} | msg: {Data}";
        }
    }
}
