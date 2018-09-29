﻿using Microsoft.EntityFrameworkCore;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Database context that is connected to PostgreSQL
    /// 
    /// This class defines the tables in the database using DBSets
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        /// <summary>
        /// Constructor: This empty constructor is required for instantiation
        /// </summary>
        /// <param name="options"></param>
        public DatabaseContext(DbContextOptions options) : base(options) {}

        public DbSet<Candle> Candles { get; set; }
    }
}