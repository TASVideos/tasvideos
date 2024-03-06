using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class VariousStringLengthConstraints : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "warnings",
			table: "user_files");

		migrationBuilder.DropColumn(
			name: "title",
			table: "user_file_comments");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "warnings",
			table: "user_files",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "title",
			table: "user_file_comments",
			type: "citext",
			maxLength: 255,
			nullable: true);
	}
}
