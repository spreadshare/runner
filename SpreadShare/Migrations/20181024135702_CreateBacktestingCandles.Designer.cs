﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SpreadShare.SupportServices;

#pragma warning disable

namespace SpreadShare.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20181024135702_CreateBacktestingCandles")]
    partial class CreateBacktestingCandles
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SpreadShare.Models.BacktestingCandle", b =>
                {
                    b.Property<long>("CreatedTimestamp")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Close");

                    b.Property<decimal>("High");

                    b.Property<decimal>("Low");

                    b.Property<decimal>("Open");

                    b.Property<string>("TradingPair");

                    b.Property<decimal>("Volume");

                    b.HasKey("CreatedTimestamp");

                    b.ToTable("Candles");
                });
#pragma warning restore
        }
    }
}
