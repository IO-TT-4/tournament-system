using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentModerators",
                columns: table => new
                {
                    ModeratorsId = table.Column<string>(type: "text", nullable: false),
                    TournamentId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentModerators", x => new { x.ModeratorsId, x.TournamentId });
                    table.ForeignKey(
                        name: "FK_TournamentModerators_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentModerators_Users_ModeratorsId",
                        column: x => x.ModeratorsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentModerators_TournamentId",
                table: "TournamentModerators",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentModerators");
        }
    }
}
