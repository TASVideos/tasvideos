using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemovePmIpAddressColumn : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "ip_address",
			table: "private_messages");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "ip_address",
			table: "private_messages",
			type: "citext",
			maxLength: 50,
			nullable: true);
	}
}
