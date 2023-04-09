using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AddGameIdToTopics : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "game_id",
			table: "forum_topics",
			type: "integer",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "ix_forum_topics_game_id",
			table: "forum_topics",
			column: "game_id");

		migrationBuilder.AddForeignKey(
			name: "fk_forum_topics_games_game_id",
			table: "forum_topics",
			column: "game_id",
			principalTable: "games",
			principalColumn: "id");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_forum_topics_games_game_id",
			table: "forum_topics");

		migrationBuilder.DropIndex(
			name: "ix_forum_topics_game_id",
			table: "forum_topics");

		migrationBuilder.DropColumn(
			name: "game_id",
			table: "forum_topics");
	}
}
