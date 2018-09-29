using Microsoft.EntityFrameworkCore;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly DatabaseContext _dbContext;

        public DatabaseMigrationService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ResponseObject Migrate()
        {
            try
            {
                _dbContext.Database.Migrate();
                return new ResponseObject(ResponseCodes.Success);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                return new ResponseObject(ResponseCodes.Error, e.Message);
            }
        }
    }
}
