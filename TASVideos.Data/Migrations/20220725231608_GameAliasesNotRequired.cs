using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GameAliasesNotRequired : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "aliases",
			table: "games",
			type: "citext",
			maxLength: 250,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 250);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "aliases",
			table: "games",
			type: "citext",
			maxLength: 250,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 250,
			oldNullable: true);
	}
}
