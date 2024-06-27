using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class TransferWeightToFlags : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "weight",
			table: "publication_classes");

		migrationBuilder.AddColumn<double>(
			name: "weight",
			table: "flags",
			type: "double precision",
			nullable: false,
			defaultValue: 0.0);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "weight",
			table: "flags");

		migrationBuilder.AddColumn<double>(
			name: "weight",
			table: "publication_classes",
			type: "double precision",
			nullable: false,
			defaultValue: 0.0);
	}
}
