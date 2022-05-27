using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations
{
	public partial class NullableRomHashes : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "sha1",
				table: "game_roms",
				type: "citext",
				maxLength: 40,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "citext",
				oldMaxLength: 40);

			migrationBuilder.AlterColumn<string>(
				name: "md5",
				table: "game_roms",
				type: "citext",
				maxLength: 32,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "citext",
				oldMaxLength: 32);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "sha1",
				table: "game_roms",
				type: "citext",
				maxLength: 40,
				nullable: false,
				defaultValue: "",
				oldClrType: typeof(string),
				oldType: "citext",
				oldMaxLength: 40,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "md5",
				table: "game_roms",
				type: "citext",
				maxLength: 32,
				nullable: false,
				defaultValue: "",
				oldClrType: typeof(string),
				oldType: "citext",
				oldMaxLength: 32,
				oldNullable: true);
		}
	}
}
