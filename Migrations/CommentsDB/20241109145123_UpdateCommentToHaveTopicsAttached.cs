using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussedApi.Migrations.CommentsDB
{
    /// <inheritdoc />
    public partial class UpdateCommentToHaveTopicsAttached : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TopicId",
                table: "Comments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Comments");
        }
    }
}
