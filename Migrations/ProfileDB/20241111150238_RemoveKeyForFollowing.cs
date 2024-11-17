using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussedApi.Migrations.ProfileDB
{
    /// <inheritdoc />
    public partial class RemoveKeyForFollowing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Following",
                table: "Following");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_Following",
                table: "Following",
                column: "UserGuid");
        }
    }
}
