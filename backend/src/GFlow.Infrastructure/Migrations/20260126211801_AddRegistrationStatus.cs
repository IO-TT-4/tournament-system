using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistrationMode",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TournamentParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Data Migration: Preserve IsWithdrawn status
            // Status=3 is Withdrawn
            migrationBuilder.Sql("UPDATE \"TournamentParticipants\" SET \"Status\" = 3 WHERE \"IsWithdrawn\" = true;");

            migrationBuilder.DropColumn(
                name: "IsWithdrawn",
                table: "TournamentParticipants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationMode",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TournamentParticipants");

            migrationBuilder.AddColumn<bool>(
                name: "IsWithdrawn",
                table: "TournamentParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
