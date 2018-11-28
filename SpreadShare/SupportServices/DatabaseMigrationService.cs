using Microsoft.EntityFrameworkCore;
using SpreadShare.Models;
using SpreadShare.Utilities;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Service for migrating the database
    /// </summary>
    internal class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMigrationService"/> class.
        /// </summary>
        /// <param name="dbContext">Context of the database to migrate</param>
        public DatabaseMigrationService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Migrate the database
        /// </summary>
        /// <returns>The result of the migration</returns>
        public ResponseObject Migrate()
        {
            try
            {
                _dbContext.Database.Migrate();
                return new ResponseObject(ResponseCode.Success);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                return new ResponseObject(ResponseCode.Error, e.Message);
            }
        }
    }
}
