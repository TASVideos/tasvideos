using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GameRomAddSystem : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "ix_game_roms_md5",
			table: "game_roms");

		migrationBuilder.DropIndex(
			name: "ix_game_roms_sha1",
			table: "game_roms");

		migrationBuilder.AddColumn<int>(
			name: "system_id",
			table: "game_roms",
			type: "integer",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "ix_game_roms_system_id",
			table: "game_roms",
			column: "system_id");

		migrationBuilder.AddForeignKey(
			name: "fk_game_roms_game_systems_system_id",
			table: "game_roms",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_game_roms_game_systems_system_id",
			table: "game_roms");

		migrationBuilder.DropIndex(
			name: "ix_game_roms_system_id",
			table: "game_roms");

		migrationBuilder.DropColumn(
			name: "system_id",
			table: "game_roms");

		migrationBuilder.CreateIndex(
			name: "ix_game_roms_md5",
			table: "game_roms",
			column: "md5",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "ix_game_roms_sha1",
			table: "game_roms",
			column: "sha1",
			unique: true);
	}
}
