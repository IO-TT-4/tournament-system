using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentStructureChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tournaments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRounds",
                table: "Tournaments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "NumberOfRounds",
                table: "Tournaments");
        }
    }
}
