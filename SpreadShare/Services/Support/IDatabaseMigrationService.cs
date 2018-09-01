using System.Threading.Tasks;

namespace SpreadShare.Services
{
    interface IDatabaseMigrationService
    {
        Task Migrate();
    }
}
