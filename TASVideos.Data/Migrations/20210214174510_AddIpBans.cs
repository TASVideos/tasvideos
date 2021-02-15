using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TASVideos.Data.Migrations
{
	public partial class AddIpBans : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "IpBans",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Mask = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_IpBans", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_IpBans_Mask",
				table: "IpBans",
				column: "Mask",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "IpBans");
		}
	}
}
