using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchResultAudits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    TournamentId = table.Column<string>(type: "text", nullable: false),
                    OldScoreA = table.Column<double>(type: "double precision", nullable: true),
                    OldScoreB = table.Column<double>(type: "double precision", nullable: true),
                    NewScoreA = table.Column<double>(type: "double precision", nullable: false),
                    NewScoreB = table.Column<double>(type: "double precision", nullable: false),
                    ModifiedByDefaultId = table.Column<string>(type: "text", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResultAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchResultAudits_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchResultAudits_Users_ModifiedByDefaultId",
                        column: x => x.ModifiedByDefaultId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchResultAudits_MatchId",
                table: "MatchResultAudits",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResultAudits_ModifiedByDefaultId",
                table: "MatchResultAudits",
                column: "ModifiedByDefaultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchResultAudits");
        }
    }
}
