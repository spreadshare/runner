using System.Threading.Tasks;

namespace SpreadShare.Services.Support
{
    interface IDatabaseMigrationService
    {
        Task Migrate();
    }
}
