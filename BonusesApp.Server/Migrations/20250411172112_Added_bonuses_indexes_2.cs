using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusesApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class Added_bonuses_indexes_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
drop index public."IX_Bonuses_Name";
create index "IX_Bonuses_Name"
    on public."Bonuses" using gin (to_tsvector('russian', LOWER("Name")));
""");

            migrationBuilder.Sql("""
drop index public."IX_Bonuses_PhoneNumber";

create index "IX_Bonuses_PhoneNumber"
    on public."Bonuses" using gin (to_tsvector('russian', "PhoneNumber"));
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
