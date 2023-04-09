using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveWikiContentId : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "fk_publications_wiki_pages_wiki_content_id",
			table: "publications");

		migrationBuilder.DropForeignKey(
			name: "fk_submissions_wiki_pages_wiki_content_id",
			table: "submissions");

		migrationBuilder.DropIndex(
			name: "ix_submissions_wiki_content_id",
			table: "submissions");

		migrationBuilder.DropIndex(
			name: "ix_publications_wiki_content_id",
			table: "publications");

		migrationBuilder.DropColumn(
			name: "wiki_content_id",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "wiki_content_id",
			table: "publications");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
			name: "wiki_content_id",
			table: "submissions",
			type: "integer",
			nullable: true);

		migrationBuilder.AddColumn<int>(
			name: "wiki_content_id",
			table: "publications",
			type: "integer",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "ix_submissions_wiki_content_id",
			table: "submissions",
			column: "wiki_content_id");

		migrationBuilder.CreateIndex(
			name: "ix_publications_wiki_content_id",
			table: "publications",
			column: "wiki_content_id");

		migrationBuilder.AddForeignKey(
			name: "fk_publications_wiki_pages_wiki_content_id",
			table: "publications",
			column: "wiki_content_id",
			principalTable: "wiki_pages",
			principalColumn: "id");

		migrationBuilder.AddForeignKey(
			name: "fk_submissions_wiki_pages_wiki_content_id",
			table: "submissions",
			column: "wiki_content_id",
			principalTable: "wiki_pages",
			principalColumn: "id");
	}
}
