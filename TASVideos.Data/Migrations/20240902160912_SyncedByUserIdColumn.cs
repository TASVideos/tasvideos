using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class SyncedByUserIdColumn : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "synced_by",
			table: "submissions");

		migrationBuilder.AddColumn<int>(
			name: "synced_by_user_id",
			table: "submissions",
			type: "integer",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "ix_submissions_synced_by_user_id",
			table: "submissions",
			column: "synced_by_user_id");

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_users_synced_by_user_id",
			table: "submissions",
			column: "synced_by_user_id",
			principalTable: "users",
			principalColumn: "id");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_submissions_users_synced_by_user_id",
			table: "submissions");

		migrationBuilder.DropIndex(
			name: "ix_submissions_synced_by_user_id",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "synced_by_user_id",
			table: "submissions");

		migrationBuilder.AddColumn<string>(
			name: "synced_by",
			table: "submissions",
			type: "citext",
			maxLength: 50,
			nullable: true);
	}
}
