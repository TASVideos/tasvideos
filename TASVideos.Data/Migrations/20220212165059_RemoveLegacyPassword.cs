using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveLegacyPassword : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "legacy_password",
			table: "users");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "legacy_password",
			table: "users",
			type: "citext",
			maxLength: 32,
			nullable: true);
	}
}
