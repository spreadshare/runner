using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    internal interface IDatabaseMigrationService
    {
        ResponseObject Migrate();
    }
}
