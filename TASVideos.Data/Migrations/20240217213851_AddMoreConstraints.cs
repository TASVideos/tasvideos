using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddMoreConstraints : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "normalized_email",
			table: "users",
			type: "citext",
			maxLength: 100,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "email",
			table: "users",
			type: "citext",
			maxLength: 100,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldNullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "normalized_email",
			table: "users",
			type: "citext",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 100);

		migrationBuilder.AlterColumn<string>(
			name: "email",
			table: "users",
			type: "citext",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 100);
	}
}
