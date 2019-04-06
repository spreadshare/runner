// <auto-generated />
using System;
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
    [Migration("20190402180336_SessionExitCode")]
    partial class SessionExitCode
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SpreadShare.Models.Database.AlgorithmSession", b =>
                {
                    b.Property<int>("SessionId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<string>("AlgorithmConfiguration");

                    b.Property<string>("AllocationJson");

                    b.Property<long>("ClosedTimestamp");

                    b.Property<string>("ContainerId");

                    b.Property<long>("CreatedTimestamp");

                    b.Property<int>("ExitCode");

                    b.Property<string>("Name");

                    b.HasKey("SessionId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.BacktestOrder", b =>
                {
                    b.Property<long>("OrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Assets");

                    b.Property<long>("CreatedTimestamp");

                    b.Property<decimal>("FilledPrice");

                    b.Property<decimal>("FilledQuantity");

                    b.Property<long>("FilledTimestamp");

                    b.Property<string>("OrderStatus");

                    b.Property<string>("OrderType");

                    b.Property<string>("Pair");

                    b.Property<decimal>("SetPrice");

                    b.Property<decimal>("SetQuantity");

                    b.Property<string>("Side");

                    b.Property<decimal>("StopPrice");

                    b.Property<long>("TradeId");

                    b.Property<decimal>("Value");

                    b.HasKey("OrderId");

                    b.ToTable("BacktestOrders");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.BacktestingCandle", b =>
                {
                    b.Property<long>("Timestamp");

                    b.Property<string>("TradingPair");

                    b.Property<decimal>("Close");

                    b.Property<decimal>("High");

                    b.Property<decimal>("Low");

                    b.Property<decimal>("Open");

                    b.Property<decimal>("Volume");

                    b.HasKey("Timestamp", "TradingPair");

                    b.ToTable("Candles");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.LogEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LogLevel");

                    b.Property<int?>("SessionId");

                    b.Property<string>("Text");

                    b.Property<long>("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("LogEvents");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.OrderEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CreatedTimestamp");

                    b.Property<long>("EventTimestamp");

                    b.Property<decimal>("FilledPrice");

                    b.Property<decimal>("FilledQuantity");

                    b.Property<long>("FilledTimestamp");

                    b.Property<long>("OrderId");

                    b.Property<string>("OrderStatus");

                    b.Property<string>("OrderType");

                    b.Property<string>("Pair");

                    b.Property<int?>("SessionId");

                    b.Property<decimal>("SetPrice");

                    b.Property<decimal>("SetQuantity");

                    b.Property<string>("Side");

                    b.Property<decimal>("StopPrice");

                    b.Property<long>("TradeId");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("OrderEvents");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.StateSwitchEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("SessionId");

                    b.Property<long>("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("StateSwitchEvents");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.LogEvent", b =>
                {
                    b.HasOne("SpreadShare.Models.Database.AlgorithmSession", "Session")
                        .WithMany()
                        .HasForeignKey("SessionId");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.OrderEvent", b =>
                {
                    b.HasOne("SpreadShare.Models.Database.AlgorithmSession", "Session")
                        .WithMany()
                        .HasForeignKey("SessionId");
                });

            modelBuilder.Entity("SpreadShare.Models.Database.StateSwitchEvent", b =>
                {
                    b.HasOne("SpreadShare.Models.Database.AlgorithmSession", "Session")
                        .WithMany()
                        .HasForeignKey("SessionId");
                });
#pragma warning restore 612, 618
        }
    }
}

#pragma warning restore
