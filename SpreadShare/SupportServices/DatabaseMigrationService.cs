using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SpreadShare.SupportServices
{
    class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly DatabaseContext _dbContext;

        public DatabaseMigrationService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Migrate()
        {
            _dbContext.Database.Migrate();
            return Task.FromResult(0);
        }
    }
}
