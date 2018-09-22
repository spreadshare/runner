namespace SpreadShare.Models
{
    public enum ResponseCodes
    {
        Error,
        NotDefined,
        Success
    }

    public class ResponseObject {
        ResponseCodes _code;
        string _message;

        public ResponseCodes Code { get { return _code; }}
        public string Message { get { return _message;}}

        public ResponseObject(ResponseCodes _code, string _message="") {
            this._code = _code;
            this._message = _message;
        }

        public override string ToString() {
            return $"{Code} | msg: {Message}";
        }
    }
}
