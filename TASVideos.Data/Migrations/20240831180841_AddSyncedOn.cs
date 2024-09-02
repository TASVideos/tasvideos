using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class AddSyncedOn : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "synced_by",
			table: "submissions",
			type: "citext",
			maxLength: 50,
			nullable: true);

		migrationBuilder.AddColumn<DateTime>(
			name: "synced_on",
			table: "submissions",
			type: "timestamp without time zone",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "synced_by",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "synced_on",
			table: "submissions");
	}
}
