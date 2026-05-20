using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TASVideos.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWikiRedirects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wiki_redirects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_name_from = table.Column<string>(type: "citext", nullable: false),
                    page_name_to = table.Column<string>(type: "citext", nullable: false),
                    create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wiki_redirects", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wiki_redirects_page_name_from",
                table: "wiki_redirects",
                column: "page_name_from",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wiki_redirects");
        }
    }
}
