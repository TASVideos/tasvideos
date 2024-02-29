using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddUserLocaleSettings : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "date_format",
			table: "users",
			type: "integer",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.AddColumn<int>(
			name: "decimal_format",
			table: "users",
			type: "integer",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.AddColumn<int>(
			name: "time_format",
			table: "users",
			type: "integer",
			nullable: false,
			defaultValue: 0);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "date_format",
			table: "users");

		migrationBuilder.DropColumn(
			name: "decimal_format",
			table: "users");

		migrationBuilder.DropColumn(
			name: "time_format",
			table: "users");
	}
}
