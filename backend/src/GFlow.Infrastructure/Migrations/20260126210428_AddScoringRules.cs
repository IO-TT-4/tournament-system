using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DrawPoints",
                table: "Tournaments",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LossPoints",
                table: "Tournaments",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WinPoints",
                table: "Tournaments",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrawPoints",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "LossPoints",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "WinPoints",
                table: "Tournaments");
        }
    }
}
