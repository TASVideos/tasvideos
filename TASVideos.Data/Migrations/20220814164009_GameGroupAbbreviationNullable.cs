using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GameGroupAbbreviationNullable : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "abbreviation",
			table: "game_groups",
			type: "citext",
			maxLength: 255,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 255);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "abbreviation",
			table: "game_groups",
			type: "citext",
			maxLength: 255,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 255,
			oldNullable: true);
	}
}
