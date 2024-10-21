using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DiscussedApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1ef50b8a-5821-46a7-a3f7-355a68ada72b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c40a494f-da89-4d3a-a09a-d4ee82658e00");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6aada918-f52a-4fa2-a289-2ae7cb8f7725", null, "Admin", "ADMIN" },
                    { "7991020e-aa58-4373-9278-1f5e00b1a915", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6aada918-f52a-4fa2-a289-2ae7cb8f7725");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7991020e-aa58-4373-9278-1f5e00b1a915");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1ef50b8a-5821-46a7-a3f7-355a68ada72b", null, "User", "USER" },
                    { "c40a494f-da89-4d3a-a09a-d4ee82658e00", null, "Admin", "ADMIN" }
                });
        }
    }
}
