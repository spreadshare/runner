using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class CreateTrades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    OrderId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TradeId = table.Column<long>(nullable: false),
                    OrderType = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<long>(nullable: false),
                    FilledTimestamp = table.Column<long>(nullable: false),
                    Pair = table.Column<string>(nullable: true),
                    SetQuantity = table.Column<decimal>(nullable: false),
                    FilledQuantity = table.Column<decimal>(nullable: false),
                    SetPrice = table.Column<decimal>(nullable: false),
                    FilledPrice = table.Column<decimal>(nullable: false),
                    Side = table.Column<string>(nullable: true),
                    Assets = table.Column<string>(nullable: true),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.OrderId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}

#pragma warning restore
