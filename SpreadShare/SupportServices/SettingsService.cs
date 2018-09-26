using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    public class SettingsService : ISettingsService
    {
        public ResponseObject Start()
        {
            return new ResponseObject(ResponseCodes.Error);
        }
    }
}