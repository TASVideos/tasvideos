using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RequireSubmitter : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_submissions_users_submitter_id",
			table: "submissions");

		migrationBuilder.AlterColumn<int>(
			name: "submitter_id",
			table: "submissions",
			type: "integer",
			nullable: false,
			defaultValue: 0,
			oldClrType: typeof(int),
			oldType: "integer",
			oldNullable: true);

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_users_submitter_id",
			table: "submissions",
			column: "submitter_id",
			principalTable: "users",
			principalColumn: "id",
			onDelete: ReferentialAction.Cascade);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_submissions_users_submitter_id",
			table: "submissions");

		migrationBuilder.AlterColumn<int>(
			name: "submitter_id",
			table: "submissions",
			type: "integer",
			nullable: true,
			oldClrType: typeof(int),
			oldType: "integer");

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_users_submitter_id",
			table: "submissions",
			column: "submitter_id",
			principalTable: "users",
			principalColumn: "id");
	}
}
