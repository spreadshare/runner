using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class RemoveFromState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "StateSwitchEvents");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "StateSwitchEvents",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "StateSwitchEvents",
                newName: "To");

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "StateSwitchEvents",
                nullable: true);
        }
    }
}

#pragma warning restore
