using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "content_type",
                table: "forum_topics",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "forum_topics");
        }
    }
}
