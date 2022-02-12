using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class EmailOnPM : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
			name: "email_on_private_message",
			table: "users",
			type: "boolean",
			nullable: false,
			defaultValue: false);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "email_on_private_message",
			table: "users");
	}
}
