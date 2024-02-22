using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class RemovePublicationBranch : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "branch",
			table: "publications");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "branch",
			table: "publications",
			type: "citext",
			maxLength: 50,
			nullable: true);
	}
}
