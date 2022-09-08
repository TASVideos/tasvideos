using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GameGroupAbbreviationUnique : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateIndex(
			name: "ix_game_groups_abbreviation",
			table: "game_groups",
			column: "abbreviation",
			unique: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "ix_game_groups_abbreviation",
			table: "game_groups");
	}
}
