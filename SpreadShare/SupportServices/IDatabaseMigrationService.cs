using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Inferface for service that migrate the database.
    /// </summary>
    internal interface IDatabaseMigrationService
    {
        /// <summary>
        /// Migrate the database.
        /// </summary>
        /// <returns>Whether the migration was successful.</returns>
        ResponseObject Migrate();
    }
}
