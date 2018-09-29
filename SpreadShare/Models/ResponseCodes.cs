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

        public ResponseObject(ResponseCodes code, T data) {
            Code = code;
            Data = data;
            Message = "";
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

    public class ResponseObject : ResponseObject<string>
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
