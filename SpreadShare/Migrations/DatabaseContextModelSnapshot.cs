﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SpreadShare.SupportServices;

namespace SpreadShare.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SpreadShare.Models.BacktestingCandle", b =>
                {
                    b.Property<long>("Timestamp")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Close");

                    b.Property<decimal>("High");

                    b.Property<decimal>("Low");

                    b.Property<decimal>("Open");

                    b.Property<string>("TradingPair");

                    b.Property<decimal>("Volume");

                    b.HasKey("Timestamp");

                    b.ToTable("Candles");
                });

            modelBuilder.Entity("SpreadShare.Models.DatabaseTrade", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Assets");

                    b.Property<string>("Pair");

                    b.Property<decimal>("Price");

                    b.Property<decimal>("Quantity");

                    b.Property<string>("Side");

                    b.Property<long>("Timestamp");

                    b.Property<decimal>("Value");

                    b.HasKey("ID");

                    b.ToTable("Trades");
                });
#pragma warning restore 612, 618
        }
    }
}
