using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddGameVersionColumns : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "notes",
			table: "game_versions",
			type: "citext",
			maxLength: 1000,
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "source_db",
			table: "game_versions",
			type: "citext",
			maxLength: 50,
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "notes",
			table: "game_versions");

		migrationBuilder.DropColumn(
			name: "source_db",
			table: "game_versions");
	}
}
