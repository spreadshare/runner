using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class UnmapDerivedProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageFilledPrice",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "FilledQuantity",
                table: "OrderEvents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageFilledPrice",
                table: "OrderEvents",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FilledQuantity",
                table: "OrderEvents",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

#pragma warning restore
