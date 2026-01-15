using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations
{
    /// <inheritdoc />
    public partial class DotNet10Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_users_normalized_user_name",
                table: "users",
                newName: "UserNameIndex");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_users_user_id",
                table: "user_claims",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_logins_users_user_id",
                table: "user_logins",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_tokens_users_user_id",
                table: "user_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_users_user_id",
                table: "user_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_logins_users_user_id",
                table: "user_logins");

            migrationBuilder.DropForeignKey(
                name: "fk_user_tokens_users_user_id",
                table: "user_tokens");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "users");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "roles");

            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "users",
                newName: "ix_users_normalized_user_name");
        }
    }
}
