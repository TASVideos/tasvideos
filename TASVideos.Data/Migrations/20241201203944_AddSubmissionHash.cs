using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddSubmissionHash : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "hash",
			table: "submissions",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "hash_type",
			table: "submissions",
			type: "citext",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "hash",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "hash_type",
			table: "submissions");
	}
}
