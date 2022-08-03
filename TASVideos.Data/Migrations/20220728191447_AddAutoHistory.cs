using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class AddAutoHistory : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "auto_history",
			columns: table => new
			{
				id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				row_id = table.Column<string>(type: "citext", maxLength: 50, nullable: false),
				table_name = table.Column<string>(type: "citext", maxLength: 128, nullable: false),
				changed = table.Column<string>(type: "citext", nullable: true),
				kind = table.Column<int>(type: "integer", nullable: false),
				created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
				discriminator = table.Column<string>(type: "text", nullable: false),
				user_id = table.Column<int>(type: "integer", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("pk_auto_history", x => x.id);
			});
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "auto_history");
	}
}
