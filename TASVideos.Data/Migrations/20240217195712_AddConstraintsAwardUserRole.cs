using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddConstraintsAwardUserRole : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "user_name",
			table: "users",
			type: "citext",
			maxLength: 50,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "normalized_user_name",
			table: "users",
			type: "citext",
			maxLength: 50,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "name",
			table: "roles",
			type: "citext",
			maxLength: 50,
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "citext",
			oldNullable: true);

		migrationBuilder.CreateIndex(
			name: "ix_awards_short_name",
			table: "awards",
			column: "short_name",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "ix_awards_short_name",
			table: "awards");

		migrationBuilder.AlterColumn<string>(
			name: "user_name",
			table: "users",
			type: "citext",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 50);

		migrationBuilder.AlterColumn<string>(
			name: "normalized_user_name",
			table: "users",
			type: "citext",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 50);

		migrationBuilder.AlterColumn<string>(
			name: "name",
			table: "roles",
			type: "citext",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "citext",
			oldMaxLength: 50);
	}
}
