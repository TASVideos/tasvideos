using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RenameGameVersionForeignKeys : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_publications_game_versions_rom_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_game_versions_rom_id",
			table: "submissions");

		migrationBuilder.RenameColumn(
			name: "rom_id",
			table: "submissions",
			newName: "game_version_id");

		migrationBuilder.RenameIndex(
			name: "ix_submissions_rom_id",
			table: "submissions",
			newName: "ix_submissions_game_version_id");

		migrationBuilder.RenameColumn(
			name: "rom_id",
			table: "publications",
			newName: "game_version_id");

		migrationBuilder.RenameIndex(
			name: "ix_publications_rom_id",
			table: "publications",
			newName: "ix_publications_game_version_id");

		migrationBuilder.AddForeignKey(
			name: "fk_publications_game_versions_game_version_id",
			table: "publications",
			column: "game_version_id",
			principalTable: "game_versions",
			principalColumn: "id",
			onDelete: ReferentialAction.Restrict);

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_game_versions_game_version_id",
			table: "submissions",
			column: "game_version_id",
			principalTable: "game_versions",
			principalColumn: "id");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_publications_game_versions_game_version_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_game_versions_game_version_id",
			table: "submissions");

		migrationBuilder.RenameColumn(
			name: "game_version_id",
			table: "submissions",
			newName: "rom_id");

		migrationBuilder.RenameIndex(
			name: "ix_submissions_game_version_id",
			table: "submissions",
			newName: "ix_submissions_rom_id");

		migrationBuilder.RenameColumn(
			name: "game_version_id",
			table: "publications",
			newName: "rom_id");

		migrationBuilder.RenameIndex(
			name: "ix_publications_game_version_id",
			table: "publications",
			newName: "ix_publications_rom_id");

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
}
