using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentFilteringAndPersonalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameCode",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameName",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Tournaments",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "Tournaments",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "Tournaments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TournamentId = table.Column<string>(type: "text", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_TournamentId",
                table: "UserActivities",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "GameCode",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "GameName",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Tournaments");
        }
    }
}
