using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GameAbbreviationUnique : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateIndex(
			name: "ix_games_abbreviation",
			table: "games",
			column: "abbreviation",
			unique: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "ix_games_abbreviation",
			table: "games");
	}
}
