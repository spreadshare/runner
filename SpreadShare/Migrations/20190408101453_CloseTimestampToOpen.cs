using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class CloseTimestampToOpen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClosedTimestamp",
                table: "Candles",
                newName: "OpenTimestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenTimestamp",
                table: "Candles",
                newName: "ClosedTimestamp");
        }
    }
}

#pragma warning restore
