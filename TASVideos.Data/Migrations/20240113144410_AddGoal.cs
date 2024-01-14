using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AddGoal : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "game_goal_id",
			table: "submissions",
			type: "integer",
			nullable: true);

		migrationBuilder.AddColumn<int>(
			name: "game_goal_id",
			table: "publications",
			type: "integer",
			nullable: true);

		migrationBuilder.CreateTable(
			name: "game_goals",
			columns: table => new
			{
				id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				game_id = table.Column<int>(type: "integer", nullable: false),
				display_name = table.Column<string>(type: "citext", maxLength: 50, nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_game_goals", x => x.id);
				table.ForeignKey(
					name: "fk_game_goals_games_game_id",
					column: x => x.game_id,
					principalTable: "games",
					principalColumn: "id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "ix_submissions_game_goal_id",
			table: "submissions",
			column: "game_goal_id");

		migrationBuilder.CreateIndex(
			name: "ix_publications_game_goal_id",
			table: "publications",
			column: "game_goal_id");

		migrationBuilder.CreateIndex(
			name: "ix_game_goals_game_id",
			table: "game_goals",
			column: "game_id");

		migrationBuilder.AddForeignKey(
			name: "fk_publications_game_goals_game_goal_id",
			table: "publications",
			column: "game_goal_id",
			principalTable: "game_goals",
			principalColumn: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_game_goals_game_goal_id",
			table: "submissions",
			column: "game_goal_id",
			principalTable: "game_goals",
			principalColumn: "id");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_publications_game_goals_game_goal_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_game_goals_game_goal_id",
			table: "submissions");

		migrationBuilder.DropTable(
			name: "game_goals");

		migrationBuilder.DropIndex(
			name: "ix_submissions_game_goal_id",
			table: "submissions");

		migrationBuilder.DropIndex(
			name: "ix_publications_game_goal_id",
			table: "publications");

		migrationBuilder.DropColumn(
			name: "game_goal_id",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "game_goal_id",
			table: "publications");
	}
}
