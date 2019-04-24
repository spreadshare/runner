using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class OrderEventProxy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestOrders");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "StateSwitchEvents",
                newName: "EventTimestamp");

            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "OrderEvents",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "FilledPrice",
                table: "OrderEvents",
                newName: "LastFillPrice");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "LogEvents",
                newName: "EventTimestamp");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OrderEvents",
                nullable: false,
                oldClrType: typeof(long))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageFilledPrice",
                table: "OrderEvents",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                table: "OrderEvents",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CommissionAsset",
                table: "OrderEvents",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastFillIncrement",
                table: "OrderEvents",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageFilledPrice",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "Commission",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "CommissionAsset",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "LastFillIncrement",
                table: "OrderEvents");

            migrationBuilder.RenameColumn(
                name: "EventTimestamp",
                table: "StateSwitchEvents",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OrderEvents",
                newName: "OrderStatus");

            migrationBuilder.RenameColumn(
                name: "LastFillPrice",
                table: "OrderEvents",
                newName: "FilledPrice");

            migrationBuilder.RenameColumn(
                name: "EventTimestamp",
                table: "LogEvents",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "OrderEvents",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.CreateTable(
                name: "BacktestOrders",
                columns: table => new
                {
                    OrderId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Assets = table.Column<string>(nullable: true),
                    CreatedTimestamp = table.Column<long>(nullable: false),
                    FilledPrice = table.Column<decimal>(nullable: false),
                    FilledQuantity = table.Column<decimal>(nullable: false),
                    FilledTimestamp = table.Column<long>(nullable: false),
                    OrderStatus = table.Column<string>(nullable: true),
                    OrderType = table.Column<string>(nullable: true),
                    Pair = table.Column<string>(nullable: true),
                    SetPrice = table.Column<decimal>(nullable: false),
                    SetQuantity = table.Column<decimal>(nullable: false),
                    Side = table.Column<string>(nullable: true),
                    StopPrice = table.Column<decimal>(nullable: false),
                    TradeId = table.Column<long>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestOrders", x => x.OrderId);
                });
        }
    }
}

#pragma warning restore
