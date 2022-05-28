using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class ColumnRenameSubmittedGameVersion : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "game_version",
			table: "submissions",
			newName: "submitted_game_version");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
			name: "submitted_game_version",
			table: "submissions",
			newName: "game_version");
	}
}