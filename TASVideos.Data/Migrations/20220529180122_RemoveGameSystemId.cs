using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveGameSystemId : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_games_game_systems_system_id",
			table: "games");

		migrationBuilder.DropIndex(
			name: "ix_games_system_id",
			table: "games");

		migrationBuilder.DropColumn(
			name: "system_id",
			table: "games");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "system_id",
			table: "games",
			type: "integer",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.CreateIndex(
			name: "ix_games_system_id",
			table: "games",
			column: "system_id");

		migrationBuilder.AddForeignKey(
			name: "fk_games_game_systems_system_id",
			table: "games",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id",
			onDelete: ReferentialAction.Cascade);
	}
}