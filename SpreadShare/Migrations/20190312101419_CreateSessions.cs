using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable

namespace SpreadShare.Migrations
{
    public partial class CreateSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
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

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "OrderEvents",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Active = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateSwitchEvents_SessionId",
                table: "StateSwitchEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvents_SessionId",
                table: "OrderEvents",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderEvents_Sessions_SessionId",
                table: "OrderEvents",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StateSwitchEvents_Sessions_SessionId",
                table: "StateSwitchEvents",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderEvents_Sessions_SessionId",
                table: "OrderEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_StateSwitchEvents_Sessions_SessionId",
                table: "StateSwitchEvents");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_StateSwitchEvents_SessionId",
                table: "StateSwitchEvents");

            migrationBuilder.DropIndex(
                name: "IX_OrderEvents_SessionId",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "StateSwitchEvents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "StateSwitchEvents");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "StateSwitchEvents");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "OrderEvents");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderEvents");
        }
    }
}

#pragma warning restore
