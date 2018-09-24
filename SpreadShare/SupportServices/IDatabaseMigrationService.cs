using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    public interface IDatabaseMigrationService
    {
        ResponseObject Migrate();
    }
}
