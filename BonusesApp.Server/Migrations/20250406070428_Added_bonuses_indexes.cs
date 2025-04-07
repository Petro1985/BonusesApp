using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class Added_bonuses_indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_Name",
                table: "Bonuses",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_PhoneNumber",
                table: "Bonuses",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bonuses_Name",
                table: "Bonuses");

            migrationBuilder.DropIndex(
                name: "IX_Bonuses_PhoneNumber",
                table: "Bonuses");
        }
    }
}
