using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicesToAutoHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_auto_history_row_id",
                table: "auto_history",
                column: "row_id");

            migrationBuilder.CreateIndex(
                name: "ix_auto_history_table_name",
                table: "auto_history",
                column: "table_name");

            migrationBuilder.CreateIndex(
                name: "ix_auto_history_user_id",
                table: "auto_history",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_auto_history_row_id",
                table: "auto_history");

            migrationBuilder.DropIndex(
                name: "ix_auto_history_table_name",
                table: "auto_history");

            migrationBuilder.DropIndex(
                name: "ix_auto_history_user_id",
                table: "auto_history");
        }
    }
}
