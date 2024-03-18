using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AddAnnotationsColumns : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "annotations",
			table: "user_files",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "annotations",
			table: "submissions",
			type: "citext",
			nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "annotations",
			table: "user_files");

		migrationBuilder.DropColumn(
			name: "annotations",
			table: "submissions");
	}
}
