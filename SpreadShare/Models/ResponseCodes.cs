namespace SpreadShare.Models
{
    internal enum ResponseCodes
    {
        Error,
        NotDefined,
        Success
    }

    internal class ResponseObject<T> {
        public ResponseCodes Code { get; }
        public string Message { get; }
        public T Data { get; }

        public bool Success => Code == ResponseCodes.Success;


        public ResponseObject(ResponseCodes code, ResponseObject flow) {
            Code = code;
            Message = flow.ToString();
        }
        public ResponseObject(ResponseCodes code, string msg) {
            Code = code;
            Message = msg;
        }

        public ResponseObject(ResponseCodes code, T data, string message = "") {
            Code = code;
            Data = data;
            Message = message;
        }

        public ResponseObject(ResponseCodes code) {
            Code = code;
            Message = "";
        }

        public override string ToString()
        {
            return $"{Code} | data: {Data} | msg: {Message}";
        }
    }

    internal class ResponseObject : ResponseObject<string>
    {
        public ResponseObject(ResponseCodes code, string data = "") : base(code, data)
        {
        }

        public override string ToString()
        {
            return $"{Code} | msg: {Message}";
        }
    }
}
