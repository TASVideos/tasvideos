using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveRatingType : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
			name: "pk_publication_ratings",
			table: "publication_ratings");

		migrationBuilder.DropIndex(
			name: "ix_publication_ratings_user_id_publication_id_type",
			table: "publication_ratings");

		migrationBuilder.DropColumn(
			name: "type",
			table: "publication_ratings");

		migrationBuilder.AddPrimaryKey(
			name: "pk_publication_ratings",
			table: "publication_ratings",
			columns: new[] { "user_id", "publication_id" });

		migrationBuilder.CreateIndex(
			name: "ix_publication_ratings_user_id_publication_id",
			table: "publication_ratings",
			columns: new[] { "user_id", "publication_id" },
			unique: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
			name: "pk_publication_ratings",
			table: "publication_ratings");

		migrationBuilder.DropIndex(
			name: "ix_publication_ratings_user_id_publication_id",
			table: "publication_ratings");

		migrationBuilder.AddColumn<int>(
			name: "type",
			table: "publication_ratings",
			type: "integer",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.AddPrimaryKey(
			name: "pk_publication_ratings",
			table: "publication_ratings",
			columns: new[] { "user_id", "publication_id", "type" });

		migrationBuilder.CreateIndex(
			name: "ix_publication_ratings_user_id_publication_id_type",
			table: "publication_ratings",
			columns: new[] { "user_id", "publication_id", "type" },
			unique: true);
	}
}
