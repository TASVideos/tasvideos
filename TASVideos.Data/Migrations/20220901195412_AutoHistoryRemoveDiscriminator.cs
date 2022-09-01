using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AutoHistoryRemoveDiscriminator : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "discriminator",
			table: "auto_history");

		migrationBuilder.AlterColumn<int>(
			name: "user_id",
			table: "auto_history",
			type: "integer",
			nullable: false,
			defaultValue: 0,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
			name: "user_id",
			table: "auto_history",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AddColumn<string>(
			name: "discriminator",
			table: "auto_history",
			type: "text",
			nullable: false,
			defaultValue: "");
	}
}
