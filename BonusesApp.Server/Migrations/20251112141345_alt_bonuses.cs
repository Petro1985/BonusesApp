using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class alt_bonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Bonuses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Bonuses");
        }
    }
}
