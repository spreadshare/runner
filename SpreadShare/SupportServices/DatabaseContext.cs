using System;
using Microsoft.EntityFrameworkCore;
using SpreadShare.Models.Database;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Database context that is connected to PostgreSQL
    /// This class defines the tables in the database using DBSets.
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// This empty constructor is required for instantiation.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the backtesting candles.
        /// </summary>
        public DbSet<BacktestingCandle> Candles { get; set; }

        /// <summary>
        /// Gets or sets the order events.
        /// </summary>
        public DbSet<OrderEvent> OrderEvents { get; set; }

        /// <summary>
        /// Gets or sets the state switch events.
        /// </summary>
        public DbSet<StateSwitchEvent> StateSwitchEvents { get; set; }

        /// <summary>
        /// Gets or sets the algorithm sessions.
        /// </summary>
        public DbSet<AlgorithmSession> Sessions { get; set; }

        /// <summary>
        /// Gets or sets the log events.
        /// </summary>
        public DbSet<LogEvent> LogEvents { get; set; }

        /// <inheritdoc />
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("#########################");
                Console.WriteLine("SaveChanges failed");
                Console.WriteLine(e);
                Console.WriteLine("#########################");
            }

            return -1;
        }

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception e)
            {
                Console.WriteLine("#########################");
                Console.WriteLine("SaveChanges failed");
                Console.WriteLine(e);
                Console.WriteLine("#########################");
            }

            return -1;
        }

        /// <summary>
        /// Callback that sets a composite key on <see cref="BacktestingCandle"/>.
        /// </summary>
        /// <param name="modelBuilder">Provided model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BacktestingCandle>()
                .HasKey(c => new { Timestamp = c.OpenTimestamp, c.TradingPair });
        }
    }
}