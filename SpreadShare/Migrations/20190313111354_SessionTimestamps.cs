using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class SessionTimestamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClosedTimestamp",
                table: "Sessions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedTimestamp",
                table: "Sessions",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedTimestamp",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CreatedTimestamp",
                table: "Sessions");
        }
    }
}

#pragma warning restore
