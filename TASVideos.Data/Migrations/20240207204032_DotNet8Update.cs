using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class DotNet8Update : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_forum_topics_submissions_submission_id1",
			table: "forum_topics");

		migrationBuilder.AddForeignKey(
			name: "fk_forum_topics_submissions_submission_id",
			table: "forum_topics",
			column: "submission_id",
			principalTable: "submissions",
			principalColumn: "id");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_forum_topics_submissions_submission_id",
			table: "forum_topics");

		migrationBuilder.AddForeignKey(
			name: "fk_forum_topics_submissions_submission_id1",
			table: "forum_topics",
			column: "submission_id",
			principalTable: "submissions",
			principalColumn: "id");
	}
}
