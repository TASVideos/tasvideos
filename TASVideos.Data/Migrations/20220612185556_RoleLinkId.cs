using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations
{
    public partial class RoleLinkId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_links_roles_role_id",
                table: "role_links");

            migrationBuilder.AlterColumn<int>(
                name: "role_id",
                table: "role_links",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_role_links_roles_role_id",
                table: "role_links",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_links_roles_role_id",
                table: "role_links");

            migrationBuilder.AlterColumn<int>(
                name: "role_id",
                table: "role_links",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_role_links_roles_role_id",
                table: "role_links",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id");
        }
    }
}
