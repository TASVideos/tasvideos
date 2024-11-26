using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

/// <inheritdoc />
public partial class RemoveTrigramIndexes : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "ix_wiki_pages_markup",
			table: "wiki_pages");

		migrationBuilder.DropIndex(
			name: "ix_forum_posts_text",
			table: "forum_posts");

		migrationBuilder.AlterDatabase()
			.Annotation("Npgsql:PostgresExtension:citext", ",,")
			.OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
			.OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterDatabase()
			.Annotation("Npgsql:PostgresExtension:citext", ",,")
			.Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
			.OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

		migrationBuilder.CreateIndex(
			name: "ix_wiki_pages_markup",
			table: "wiki_pages",
			column: "markup")
			.Annotation("Npgsql:IndexMethod", "gin")
			.Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

		migrationBuilder.CreateIndex(
			name: "ix_forum_posts_text",
			table: "forum_posts",
			column: "text")
			.Annotation("Npgsql:IndexMethod", "gin")
			.Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
	}
}
