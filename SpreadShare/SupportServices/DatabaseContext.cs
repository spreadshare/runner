using Microsoft.EntityFrameworkCore;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Database context that is connected to PostgreSQL
    /// This class defines the tables in the database using DBSets
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// This empty constructor is required for instantiation
        /// </summary>
        /// <param name="options">Configuration options</param>
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
        }
        
        public DbSet<DBCandle> Candles { get; set; }
    }
}