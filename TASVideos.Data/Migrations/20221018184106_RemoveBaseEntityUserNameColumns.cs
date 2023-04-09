using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class RemoveBaseEntityUserNameColumns : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "wiki_pages");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "wiki_pages");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "users");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "users");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "user_disallows");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "user_disallows");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "submissions");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "submission_status_history");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "submission_status_history");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "roles");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "roles");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "publications");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "publications");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "publication_urls");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "publication_urls");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "publication_files");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "publication_files");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "private_messages");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "private_messages");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "media_posts");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "media_posts");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "ip_bans");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "ip_bans");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "games");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "games");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "game_versions");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "game_versions");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "game_systems");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "game_systems");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "game_system_frame_rates");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "game_system_frame_rates");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forums");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forums");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forum_topics");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forum_topics");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forum_posts");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forum_posts");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forum_polls");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forum_polls");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forum_poll_options");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forum_poll_options");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "forum_categories");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "forum_categories");

		migrationBuilder.DropColumn(
			name: "create_timestamp",
			table: "flags");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "flags");

		migrationBuilder.DropColumn(
			name: "last_update_timestamp",
			table: "flags");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "flags");

		migrationBuilder.DropColumn(
			name: "create_user_name",
			table: "deprecated_movie_formats");

		migrationBuilder.DropColumn(
			name: "last_update_user_name",
			table: "deprecated_movie_formats");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "wiki_pages",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "wiki_pages",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "users",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "users",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "user_disallows",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "user_disallows",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "submissions",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "submissions",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "submission_status_history",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "submission_status_history",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "roles",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "roles",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "publications",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "publications",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "publication_urls",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "publication_urls",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "publication_files",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "publication_files",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "private_messages",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "private_messages",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "media_posts",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "media_posts",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "ip_bans",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "ip_bans",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "games",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "games",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "game_versions",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "game_versions",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "game_systems",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "game_systems",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "game_system_frame_rates",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "game_system_frame_rates",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forums",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forums",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forum_topics",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forum_topics",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forum_posts",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forum_posts",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forum_polls",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forum_polls",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forum_poll_options",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forum_poll_options",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "forum_categories",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "forum_categories",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<DateTime>(
			name: "create_timestamp",
			table: "flags",
			type: "timestamp without time zone",
			nullable: false,
			defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "flags",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<DateTime>(
			name: "last_update_timestamp",
			table: "flags",
			type: "timestamp without time zone",
			nullable: false,
			defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "flags",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "create_user_name",
			table: "deprecated_movie_formats",
			type: "citext",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "last_update_user_name",
			table: "deprecated_movie_formats",
			type: "citext",
			nullable: true);
	}
}
