using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class CreateBacktestingCandles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candles",
                columns: table => new
                {
                    Timestamp = table.Column<long>(nullable: false),
                    TradingPair = table.Column<string>(nullable: false),
                    Open = table.Column<decimal>(nullable: false),
                    Close = table.Column<decimal>(nullable: false),
                    High = table.Column<decimal>(nullable: false),
                    Low = table.Column<decimal>(nullable: false),
                    Volume = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candles", x => new { x.Timestamp, x.TradingPair });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candles");
        }
    }
}

#pragma warning restore
