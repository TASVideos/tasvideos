using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class TableRenameGameVersions : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_game_roms_game_systems_system_id",
			table: "game_roms");

		migrationBuilder.DropForeignKey(
			name: "fk_game_roms_games_game_id",
			table: "game_roms");

		migrationBuilder.DropForeignKey(
			name: "fk_publications_game_roms_rom_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_game_roms_rom_id",
			table: "submissions");

		migrationBuilder.DropPrimaryKey(
			name: "pk_game_roms",
			table: "game_roms");

		migrationBuilder.RenameTable(
			name: "game_roms",
			newName: "game_versions");

		migrationBuilder.RenameIndex(
			name: "ix_game_roms_system_id",
			table: "game_versions",
			newName: "ix_game_versions_system_id");

		migrationBuilder.RenameIndex(
			name: "ix_game_roms_game_id",
			table: "game_versions",
			newName: "ix_game_versions_game_id");

		migrationBuilder.AddPrimaryKey(
			name: "pk_game_versions",
			table: "game_versions",
			column: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_game_versions_games_game_id",
			table: "game_versions",
			column: "game_id",
			principalTable: "games",
			principalColumn: "id",
			onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
			name: "fk_publications_game_versions_rom_id",
			table: "publications",
			column: "rom_id",
			principalTable: "game_versions",
			principalColumn: "id",
			onDelete: ReferentialAction.Restrict);

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_game_versions_rom_id",
			table: "submissions",
			column: "rom_id",
			principalTable: "game_versions",
			principalColumn: "id");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_game_versions_game_systems_system_id",
			table: "game_versions");

		migrationBuilder.DropForeignKey(
			name: "fk_game_versions_games_game_id",
			table: "game_versions");

		migrationBuilder.DropForeignKey(
			name: "fk_publications_game_versions_rom_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_game_versions_rom_id",
			table: "submissions");

		migrationBuilder.DropPrimaryKey(
			name: "pk_game_versions",
			table: "game_versions");

		migrationBuilder.RenameTable(
			name: "game_versions",
			newName: "game_roms");

		migrationBuilder.RenameIndex(
			name: "ix_game_versions_system_id",
			table: "game_roms",
			newName: "ix_game_roms_system_id");

		migrationBuilder.RenameIndex(
			name: "ix_game_versions_game_id",
			table: "game_roms",
			newName: "ix_game_roms_game_id");

		migrationBuilder.AddPrimaryKey(
			name: "pk_game_roms",
			table: "game_roms",
			column: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_game_roms_game_systems_system_id",
			table: "game_roms",
			column: "system_id",
			principalTable: "game_systems",
			principalColumn: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_game_roms_games_game_id",
			table: "game_roms",
			column: "game_id",
			principalTable: "games",
			principalColumn: "id",
			onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
			name: "fk_publications_game_roms_rom_id",
			table: "publications",
			column: "rom_id",
			principalTable: "game_roms",
			principalColumn: "id",
			onDelete: ReferentialAction.Restrict);

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_game_roms_rom_id",
			table: "submissions",
			column: "rom_id",
			principalTable: "game_roms",
			principalColumn: "id");
	}
}
