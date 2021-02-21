using Microsoft.EntityFrameworkCore.Migrations;

namespace TASVideos.Data.Migrations
{
	public partial class ConstraintCleanup : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "PageNameIndex",
				table: "WikiPages");

			migrationBuilder.DropIndex(
				name: "UserDisallowRegexPatternIndex",
				table: "UserDisallows");

			migrationBuilder.DropIndex(
				name: "EmailIndex",
				table: "User");

			migrationBuilder.DropIndex(
				name: "UserNameIndex",
				table: "User");

			migrationBuilder.DropIndex(
				name: "IX_Tiers_Name",
				table: "Tiers");

			migrationBuilder.DropIndex(
				name: "IX_Tags_Code",
				table: "Tags");

			migrationBuilder.DropIndex(
				name: "IX_GameSystems_Code",
				table: "GameSystems");

			migrationBuilder.DropIndex(
				name: "IX_GameRoms_Md5",
				table: "GameRoms");

			migrationBuilder.DropIndex(
				name: "IX_GameRoms_Sha1",
				table: "GameRoms");

			migrationBuilder.DropIndex(
				name: "PageNameIndex",
				table: "ForumTopics");

			migrationBuilder.AlterColumn<string>(
				name: "NormalizedEmail",
				table: "User",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(450)",
				oldNullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_WikiPages_PageName_Revision",
				table: "WikiPages",
				columns: new[] { "PageName", "Revision" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserDisallows_RegexPattern",
				table: "UserDisallows",
				column: "RegexPattern",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_User_NormalizedUserName",
				table: "User",
				column: "NormalizedUserName",
				unique: true,
				filter: "[NormalizedUserName] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_Tiers_Name",
				table: "Tiers",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Code",
				table: "Tags",
				column: "Code",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_GameSystems_Code",
				table: "GameSystems",
				column: "Code",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_GameRoms_Md5",
				table: "GameRoms",
				column: "Md5",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_GameRoms_Sha1",
				table: "GameRoms",
				column: "Sha1",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_ForumTopics_PageName",
				table: "ForumTopics",
				column: "PageName",
				unique: true,
				filter: "[PageName] IS NOT NULL");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_WikiPages_PageName_Revision",
				table: "WikiPages");

			migrationBuilder.DropIndex(
				name: "IX_UserDisallows_RegexPattern",
				table: "UserDisallows");

			migrationBuilder.DropIndex(
				name: "IX_User_NormalizedUserName",
				table: "User");

			migrationBuilder.DropIndex(
				name: "IX_Tiers_Name",
				table: "Tiers");

			migrationBuilder.DropIndex(
				name: "IX_Tags_Code",
				table: "Tags");

			migrationBuilder.DropIndex(
				name: "IX_GameSystems_Code",
				table: "GameSystems");

			migrationBuilder.DropIndex(
				name: "IX_GameRoms_Md5",
				table: "GameRoms");

			migrationBuilder.DropIndex(
				name: "IX_GameRoms_Sha1",
				table: "GameRoms");

			migrationBuilder.DropIndex(
				name: "IX_ForumTopics_PageName",
				table: "ForumTopics");

			migrationBuilder.AlterColumn<string>(
				name: "NormalizedEmail",
				table: "User",
				type: "nvarchar(450)",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);

			migrationBuilder.CreateIndex(
				name: "PageNameIndex",
				table: "WikiPages",
				columns: new[] { "PageName", "Revision" },
				unique: true,
				filter: "([PageName] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "UserDisallowRegexPatternIndex",
				table: "UserDisallows",
				column: "RegexPattern",
				unique: true,
				filter: "([RegexPattern] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "EmailIndex",
				table: "User",
				column: "NormalizedEmail");

			migrationBuilder.CreateIndex(
				name: "UserNameIndex",
				table: "User",
				column: "NormalizedUserName",
				unique: true,
				filter: "([NormalizedUserName] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_Tiers_Name",
				table: "Tiers",
				column: "Name",
				unique: true,
				filter: "([Name] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Code",
				table: "Tags",
				column: "Code",
				unique: true,
				filter: "([Code] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_GameSystems_Code",
				table: "GameSystems",
				column: "Code",
				unique: true,
				filter: "([Code] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_GameRoms_Md5",
				table: "GameRoms",
				column: "Md5",
				unique: true,
				filter: "([Sha1] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_GameRoms_Sha1",
				table: "GameRoms",
				column: "Sha1",
				unique: true,
				filter: "([Sha1] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "PageNameIndex",
				table: "ForumTopics",
				column: "PageName",
				unique: true,
				filter: "([PageName] IS NOT NULL)");
		}
	}
}
