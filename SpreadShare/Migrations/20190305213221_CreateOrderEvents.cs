using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class CreateOrderEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    OrderId = table.Column<long>(nullable: false),
                    TradeId = table.Column<long>(nullable: false),
                    OrderType = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<long>(nullable: false),
                    FilledTimestamp = table.Column<long>(nullable: false),
                    Pair = table.Column<string>(nullable: true),
                    SetQuantity = table.Column<decimal>(nullable: false),
                    FilledQuantity = table.Column<decimal>(nullable: false),
                    SetPrice = table.Column<decimal>(nullable: false),
                    StopPrice = table.Column<decimal>(nullable: false),
                    FilledPrice = table.Column<decimal>(nullable: false),
                    EventTimestamp = table.Column<long>(nullable: false),
                    Side = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvents", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderEvents");
        }
    }
}

#pragma warning restore
