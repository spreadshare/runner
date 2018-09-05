using System.Threading.Tasks;

namespace SpreadShare.SupportServices
{
    interface IDatabaseMigrationService
    {
        Task Migrate();
    }
}
