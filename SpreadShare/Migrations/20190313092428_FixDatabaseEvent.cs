using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class FixDatabaseEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "StateSwitchEvents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "StateSwitchEvents");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrderEvents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "StateSwitchEvents",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "StateSwitchEvents",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "OrderEvents",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OrderEvents",
                nullable: true);
        }
    }
}

#pragma warning restore
