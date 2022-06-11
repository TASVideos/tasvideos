using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RequireGameVersionSystemId : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions");

		migrationBuilder.AlterColumn<int>(
			name: "system_id",
			table: "game_versions",
			type: "integer",
			nullable: false,
			defaultValue: 0,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AddForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id",
			onDelete: ReferentialAction.Cascade);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions");

		migrationBuilder.AlterColumn<int>(
			name: "system_id",
			table: "game_versions",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AddForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id");
	}
}
