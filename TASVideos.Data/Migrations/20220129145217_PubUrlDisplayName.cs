using Microsoft.EntityFrameworkCore.Migrations;

namespace TASVideos.Data.Migrations;

public partial class PubUrlDisplayName : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "display_name",
			table: "publication_urls",
			type: "citext",
			maxLength: 100,
			nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "display_name",
			table: "publication_urls");
	}
}
