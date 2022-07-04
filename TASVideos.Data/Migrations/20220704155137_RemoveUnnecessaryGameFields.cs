using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveUnnecessaryGameFields : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "good_name",
			table: "games");

		migrationBuilder.DropColumn(
			name: "search_key",
			table: "games");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "good_name",
			table: "games",
			type: "citext",
			maxLength: 250,
			nullable: false,
			defaultValue: "");

		migrationBuilder.AddColumn<string>(
			name: "search_key",
			table: "games",
			type: "citext",
			maxLength: 64,
			nullable: true);
	}
}
