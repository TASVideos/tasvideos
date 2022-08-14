using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RenameGameGroupSearchKey : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "search_key",
			table: "game_groups",
			newName: "abbreviation");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "abbreviation",
			table: "game_groups",
			newName: "search_key");
	}
}
