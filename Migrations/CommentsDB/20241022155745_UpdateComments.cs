using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussedApi.Migrations.CommentsDB
{
    /// <inheritdoc />
    public partial class UpdateComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DtCreated",
                table: "Replys",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtUpdated",
                table: "Replys",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtCreated",
                table: "Comments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DtUpdated",
                table: "Comments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DtCreated",
                table: "Replys");

            migrationBuilder.DropColumn(
                name: "DtUpdated",
                table: "Replys");

            migrationBuilder.DropColumn(
                name: "DtCreated",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DtUpdated",
                table: "Comments");
        }
    }
}
