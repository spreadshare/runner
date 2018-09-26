using Microsoft.Extensions.Configuration;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    public class SettingsService : ISettingsService
    {
        IConfiguration _configuration;
        public SettingsService(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
        public ResponseObject Start()
        {
            return new ResponseObject(ResponseCodes.Error);
        }
    }
}