using Microsoft.EntityFrameworkCore;
using SpreadShare.Models;

namespace SpreadShare.DependencyInjection
{
    /// <summary>
    /// Database context that is connected to PostgreSQL
    /// 
    /// This class defines the tables in the database using DBSets
    /// </summary>
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Constructor: This empty constructor is required for instantiation
        /// </summary>
        /// <param name="options"></param>
        public DatabaseContext(DbContextOptions options) : base(options) {}

        public DbSet<Candle> Candles { get; set; }
    }
}