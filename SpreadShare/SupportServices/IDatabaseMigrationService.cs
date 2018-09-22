using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    interface IDatabaseMigrationService
    {
        ResponseObject Migrate();
    }
}
