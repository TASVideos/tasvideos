using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace TASVideos.Data.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterDatabase()
				.Annotation("Npgsql:PostgresExtension:citext", ",,")
				.Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

			migrationBuilder.CreateTable(
				name: "awards",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					type = table.Column<int>(type: "integer", nullable: false),
					short_name = table.Column<string>(type: "citext", maxLength: 25, nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_awards", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "deprecated_movie_formats",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					file_extension = table.Column<string>(type: "citext", nullable: false),
					deprecated = table.Column<bool>(type: "boolean", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_deprecated_movie_formats", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "flags",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					name = table.Column<string>(type: "citext", maxLength: 32, nullable: false),
					icon_path = table.Column<string>(type: "citext", maxLength: 48, nullable: true),
					link_path = table.Column<string>(type: "citext", maxLength: 48, nullable: true),
					token = table.Column<string>(type: "citext", maxLength: 24, nullable: false),
					permission_restriction = table.Column<int>(type: "integer", nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_flags", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "forum_categories",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					title = table.Column<string>(type: "citext", maxLength: 30, nullable: false),
					ordinal = table.Column<int>(type: "integer", nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 1000, nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_categories", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "forum_polls",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					topic_id = table.Column<int>(type: "integer", nullable: false),
					question = table.Column<string>(type: "citext", maxLength: 500, nullable: false),
					close_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
					multi_select = table.Column<bool>(type: "boolean", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_polls", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "game_groups",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					search_key = table.Column<string>(type: "citext", maxLength: 255, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_groups", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "game_systems",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					code = table.Column<string>(type: "citext", maxLength: 8, nullable: false),
					display_name = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_systems", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "genres",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					display_name = table.Column<string>(type: "citext", maxLength: 20, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_genres", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "ip_bans",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					mask = table.Column<string>(type: "citext", maxLength: 40, nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_ip_bans", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "media_posts",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					title = table.Column<string>(type: "citext", maxLength: 512, nullable: false),
					link = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					body = table.Column<string>(type: "citext", maxLength: 1024, nullable: false),
					group = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					type = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
					user = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_media_posts", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "publication_classes",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					name = table.Column<string>(type: "citext", maxLength: 20, nullable: false),
					weight = table.Column<double>(type: "double precision", nullable: false),
					icon_path = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
					link = table.Column<string>(type: "citext", maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_classes", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "role_claims",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					role_id = table.Column<int>(type: "integer", nullable: false),
					claim_type = table.Column<string>(type: "citext", nullable: true),
					claim_value = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_role_claims", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "roles",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					is_default = table.Column<bool>(type: "boolean", nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 300, nullable: false),
					auto_assign_post_count = table.Column<int>(type: "integer", nullable: true),
					auto_assign_publications = table.Column<bool>(type: "boolean", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true),
					name = table.Column<string>(type: "citext", nullable: true),
					normalized_name = table.Column<string>(type: "citext", nullable: true),
					concurrency_stamp = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_roles", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "submission_rejection_reasons",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					display_name = table.Column<string>(type: "citext", maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_submission_rejection_reasons", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "tags",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					code = table.Column<string>(type: "citext", maxLength: 25, nullable: false),
					display_name = table.Column<string>(type: "citext", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_tags", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "user_claims",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					user_id = table.Column<int>(type: "integer", nullable: false),
					claim_type = table.Column<string>(type: "citext", nullable: true),
					claim_value = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_claims", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "user_disallows",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					regex_pattern = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_disallows", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "user_logins",
				columns: table => new
				{
					login_provider = table.Column<string>(type: "citext", nullable: false),
					provider_key = table.Column<string>(type: "citext", nullable: false),
					provider_display_name = table.Column<string>(type: "citext", nullable: true),
					user_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
				});

			migrationBuilder.CreateTable(
				name: "user_tokens",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					login_provider = table.Column<string>(type: "citext", nullable: false),
					name = table.Column<string>(type: "citext", nullable: false),
					value = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
				});

			migrationBuilder.CreateTable(
				name: "users",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					last_logged_in_time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
					time_zone_id = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true),
					avatar = table.Column<string>(type: "citext", maxLength: 250, nullable: true),
					from = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
					signature = table.Column<string>(type: "citext", maxLength: 1000, nullable: true),
					public_ratings = table.Column<bool>(type: "boolean", nullable: false),
					mood_avatar_url_base = table.Column<string>(type: "citext", maxLength: 250, nullable: true),
					use_ratings = table.Column<bool>(type: "boolean", nullable: false),
					preferred_pronouns = table.Column<int>(type: "integer", nullable: false),
					legacy_password = table.Column<string>(type: "citext", maxLength: 32, nullable: true),
					user_name = table.Column<string>(type: "citext", nullable: true),
					normalized_user_name = table.Column<string>(type: "citext", nullable: true),
					email = table.Column<string>(type: "citext", nullable: true),
					normalized_email = table.Column<string>(type: "citext", nullable: true),
					email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
					password_hash = table.Column<string>(type: "citext", nullable: true),
					security_stamp = table.Column<string>(type: "citext", nullable: true),
					concurrency_stamp = table.Column<string>(type: "citext", nullable: true),
					phone_number = table.Column<string>(type: "citext", nullable: true),
					phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
					two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
					lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
					lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
					access_failed_count = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_users", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "wiki_referrals",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					referrer = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					referral = table.Column<string>(type: "citext", maxLength: 1000, nullable: false),
					excerpt = table.Column<string>(type: "citext", maxLength: 1000, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_wiki_referrals", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "forums",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					category_id = table.Column<int>(type: "integer", nullable: false),
					name = table.Column<string>(type: "citext", maxLength: 50, nullable: false),
					short_name = table.Column<string>(type: "citext", maxLength: 10, nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 1000, nullable: true),
					ordinal = table.Column<int>(type: "integer", nullable: false),
					restricted = table.Column<bool>(type: "boolean", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forums", x => x.id);
					table.ForeignKey(
						name: "fk_forums_forum_categories_category_id",
						column: x => x.category_id,
						principalTable: "forum_categories",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "forum_poll_options",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					text = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					ordinal = table.Column<int>(type: "integer", nullable: false),
					poll_id = table.Column<int>(type: "integer", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_poll_options", x => x.id);
					table.ForeignKey(
						name: "fk_forum_poll_options_forum_polls_poll_id",
						column: x => x.poll_id,
						principalTable: "forum_polls",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "game_ram_address_domains",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					game_system_id = table.Column<int>(type: "integer", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_ram_address_domains", x => x.id);
					table.ForeignKey(
						name: "fk_game_ram_address_domains_game_systems_game_system_id",
						column: x => x.game_system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "game_system_frame_rates",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					game_system_id = table.Column<int>(type: "integer", nullable: false),
					frame_rate = table.Column<double>(type: "double precision", nullable: false),
					region_code = table.Column<string>(type: "citext", maxLength: 8, nullable: false),
					preliminary = table.Column<bool>(type: "boolean", nullable: false),
					obsolete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_system_frame_rates", x => x.id);
					table.ForeignKey(
						name: "fk_game_system_frame_rates_game_systems_game_system_id",
						column: x => x.game_system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "games",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					system_id = table.Column<int>(type: "integer", nullable: false),
					good_name = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					display_name = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
					abbreviation = table.Column<string>(type: "citext", maxLength: 8, nullable: true),
					search_key = table.Column<string>(type: "citext", maxLength: 64, nullable: true),
					youtube_tags = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					screenshot_url = table.Column<string>(type: "citext", maxLength: 250, nullable: true),
					game_resources_page = table.Column<string>(type: "citext", maxLength: 300, nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_games", x => x.id);
					table.ForeignKey(
						name: "fk_games_game_systems_system_id",
						column: x => x.system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "role_links",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					link = table.Column<string>(type: "citext", maxLength: 300, nullable: false),
					role_id = table.Column<int>(type: "integer", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_role_links", x => x.id);
					table.ForeignKey(
						name: "fk_role_links_roles_role_id",
						column: x => x.role_id,
						principalTable: "roles",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "role_permission",
				columns: table => new
				{
					role_id = table.Column<int>(type: "integer", nullable: false),
					permission_id = table.Column<int>(type: "integer", nullable: false),
					can_assign = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_role_permission", x => new { x.role_id, x.permission_id });
					table.ForeignKey(
						name: "fk_role_permission_roles_role_id",
						column: x => x.role_id,
						principalTable: "roles",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "private_messages",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					from_user_id = table.Column<int>(type: "integer", nullable: false),
					to_user_id = table.Column<int>(type: "integer", nullable: false),
					ip_address = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					subject = table.Column<string>(type: "citext", maxLength: 500, nullable: true),
					text = table.Column<string>(type: "citext", nullable: false),
					enable_html = table.Column<bool>(type: "boolean", nullable: false),
					enable_bb_code = table.Column<bool>(type: "boolean", nullable: false),
					read_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
					saved_for_from_user = table.Column<bool>(type: "boolean", nullable: false),
					saved_for_to_user = table.Column<bool>(type: "boolean", nullable: false),
					deleted_for_from_user = table.Column<bool>(type: "boolean", nullable: false),
					deleted_for_to_user = table.Column<bool>(type: "boolean", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_private_messages", x => x.id);
					table.ForeignKey(
						name: "fk_private_messages_users_from_user_id",
						column: x => x.from_user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_private_messages_users_to_user_id",
						column: x => x.to_user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "user_awards",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					user_id = table.Column<int>(type: "integer", nullable: false),
					award_id = table.Column<int>(type: "integer", nullable: false),
					year = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_awards", x => x.id);
					table.ForeignKey(
						name: "fk_user_awards_awards_award_id",
						column: x => x.award_id,
						principalTable: "awards",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_user_awards_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "user_maintenance_logs",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					log = table.Column<string>(type: "citext", nullable: false),
					editor_id = table.Column<int>(type: "integer", nullable: true),
					user_id = table.Column<int>(type: "integer", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_maintenance_logs", x => x.id);
					table.ForeignKey(
						name: "fk_user_maintenance_logs_users_editor_id",
						column: x => x.editor_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_user_maintenance_logs_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "user_roles",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					role_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
					table.ForeignKey(
						name: "fk_user_roles_roles_role_id",
						column: x => x.role_id,
						principalTable: "roles",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_user_roles_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "wiki_pages",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					page_name = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					markup = table.Column<string>(type: "citext", nullable: false),
					revision = table.Column<int>(type: "integer", nullable: false),
					minor_edit = table.Column<bool>(type: "boolean", nullable: false),
					revision_message = table.Column<string>(type: "citext", maxLength: 1000, nullable: true),
					child_id = table.Column<int>(type: "integer", nullable: true),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
						.Annotation("Npgsql:TsVectorConfig", "english")
						.Annotation("Npgsql:TsVectorProperties", new[] { "page_name", "markup" }),
					author_id = table.Column<int>(type: "integer", nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_wiki_pages", x => x.id);
					table.ForeignKey(
						name: "fk_wiki_pages_users_author_id",
						column: x => x.author_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_wiki_pages_wiki_pages_child_id",
						column: x => x.child_id,
						principalTable: "wiki_pages",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "forum_poll_option_votes",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					poll_option_id = table.Column<int>(type: "integer", nullable: false),
					user_id = table.Column<int>(type: "integer", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					ip_address = table.Column<string>(type: "citext", maxLength: 50, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_poll_option_votes", x => x.id);
					table.ForeignKey(
						name: "fk_forum_poll_option_votes_forum_poll_options_poll_option_id",
						column: x => x.poll_option_id,
						principalTable: "forum_poll_options",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_forum_poll_option_votes_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "game_game_groups",
				columns: table => new
				{
					game_id = table.Column<int>(type: "integer", nullable: false),
					game_group_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_game_groups", x => new { x.game_id, x.game_group_id });
					table.ForeignKey(
						name: "fk_game_game_groups_game_groups_game_group_id",
						column: x => x.game_group_id,
						principalTable: "game_groups",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_game_game_groups_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "game_genres",
				columns: table => new
				{
					game_id = table.Column<int>(type: "integer", nullable: false),
					genre_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_genres", x => new { x.game_id, x.genre_id });
					table.ForeignKey(
						name: "fk_game_genres_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_game_genres_genres_genre_id",
						column: x => x.genre_id,
						principalTable: "genres",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "game_ram_addresses",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					legacy_set_id = table.Column<int>(type: "integer", nullable: false),
					address = table.Column<long>(type: "bigint", nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					signed = table.Column<int>(type: "integer", nullable: false),
					endian = table.Column<int>(type: "integer", nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					game_ram_address_domain_id = table.Column<int>(type: "integer", nullable: false),
					game_id = table.Column<int>(type: "integer", nullable: true),
					legacy_game_name = table.Column<string>(type: "citext", maxLength: 255, nullable: true),
					system_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_ram_addresses", x => x.id);
					table.ForeignKey(
						name: "fk_game_ram_addresses_game_ram_address_domains_game_ram_addres",
						column: x => x.game_ram_address_domain_id,
						principalTable: "game_ram_address_domains",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_game_ram_addresses_game_systems_system_id",
						column: x => x.system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_game_ram_addresses_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "game_roms",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					game_id = table.Column<int>(type: "integer", nullable: false),
					md5 = table.Column<string>(type: "citext", maxLength: 32, nullable: false),
					sha1 = table.Column<string>(type: "citext", maxLength: 40, nullable: false),
					name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					region = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					version = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_game_roms", x => x.id);
					table.ForeignKey(
						name: "fk_game_roms_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "user_files",
				columns: table => new
				{
					id = table.Column<long>(type: "bigint", nullable: false),
					author_id = table.Column<int>(type: "integer", nullable: false),
					file_name = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					content = table.Column<byte[]>(type: "bytea", nullable: false),
					@class = table.Column<int>(name: "class", type: "integer", nullable: false),
					type = table.Column<string>(type: "citext", maxLength: 16, nullable: false),
					upload_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					length = table.Column<decimal>(type: "decimal(10, 3)", nullable: false),
					frames = table.Column<int>(type: "integer", nullable: false),
					rerecords = table.Column<int>(type: "integer", nullable: false),
					title = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
					description = table.Column<string>(type: "citext", nullable: true),
					logical_length = table.Column<int>(type: "integer", nullable: false),
					physical_length = table.Column<int>(type: "integer", nullable: false),
					game_id = table.Column<int>(type: "integer", nullable: true),
					system_id = table.Column<int>(type: "integer", nullable: true),
					hidden = table.Column<bool>(type: "boolean", nullable: false),
					warnings = table.Column<string>(type: "citext", nullable: true),
					views = table.Column<int>(type: "integer", nullable: false),
					downloads = table.Column<int>(type: "integer", nullable: false),
					compression_type = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_files", x => x.id);
					table.ForeignKey(
						name: "fk_user_files_game_systems_system_id",
						column: x => x.system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_user_files_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_user_files_users_author_id",
						column: x => x.author_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "submissions",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					wiki_content_id = table.Column<int>(type: "integer", nullable: true),
					topic_id = table.Column<int>(type: "integer", nullable: true),
					submitter_id = table.Column<int>(type: "integer", nullable: true),
					intended_class_id = table.Column<int>(type: "integer", nullable: true),
					judge_id = table.Column<int>(type: "integer", nullable: true),
					publisher_id = table.Column<int>(type: "integer", nullable: true),
					status = table.Column<int>(type: "integer", nullable: false),
					movie_file = table.Column<byte[]>(type: "bytea", nullable: false),
					movie_extension = table.Column<string>(type: "citext", nullable: true),
					game_id = table.Column<int>(type: "integer", nullable: true),
					rom_id = table.Column<int>(type: "integer", nullable: true),
					system_id = table.Column<int>(type: "integer", nullable: true),
					system_frame_rate_id = table.Column<int>(type: "integer", nullable: true),
					frames = table.Column<int>(type: "integer", nullable: false),
					rerecord_count = table.Column<int>(type: "integer", nullable: false),
					encode_embed_link = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
					game_version = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
					game_name = table.Column<string>(type: "citext", maxLength: 100, nullable: true),
					branch = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					rom_name = table.Column<string>(type: "citext", maxLength: 250, nullable: true),
					emulator_version = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					movie_start_type = table.Column<int>(type: "integer", nullable: true),
					rejection_reason_id = table.Column<int>(type: "integer", nullable: true),
					additional_authors = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
					title = table.Column<string>(type: "citext", nullable: false),
					legacy_time = table.Column<decimal>(type: "decimal(16, 4)", nullable: false, defaultValue: 0m),
					imported_time = table.Column<decimal>(type: "decimal(16, 4)", nullable: false, defaultValue: 0m),
					legacy_alerts = table.Column<string>(type: "citext", maxLength: 4096, nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_submissions", x => x.id);
					table.ForeignKey(
						name: "fk_submissions_game_roms_rom_id",
						column: x => x.rom_id,
						principalTable: "game_roms",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_game_system_frame_rates_system_frame_rate_id",
						column: x => x.system_frame_rate_id,
						principalTable: "game_system_frame_rates",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_game_systems_system_id",
						column: x => x.system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_publication_classes_intended_class_id",
						column: x => x.intended_class_id,
						principalTable: "publication_classes",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_submission_rejection_reasons_rejection_reason_id",
						column: x => x.rejection_reason_id,
						principalTable: "submission_rejection_reasons",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_users_judge_id",
						column: x => x.judge_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_users_publisher_id",
						column: x => x.publisher_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_users_submitter_id",
						column: x => x.submitter_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_submissions_wiki_pages_wiki_content_id",
						column: x => x.wiki_content_id,
						principalTable: "wiki_pages",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "user_file_comments",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					user_file_id = table.Column<long>(type: "bigint", nullable: false),
					ip = table.Column<string>(type: "citext", maxLength: 255, nullable: true),
					parent_id = table.Column<int>(type: "integer", nullable: true),
					title = table.Column<string>(type: "citext", maxLength: 255, nullable: true),
					text = table.Column<string>(type: "citext", nullable: false),
					creation_time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					user_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_file_comments", x => x.id);
					table.ForeignKey(
						name: "fk_user_file_comments_user_file_comments_parent_id",
						column: x => x.parent_id,
						principalTable: "user_file_comments",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_user_file_comments_user_files_user_file_id",
						column: x => x.user_file_id,
						principalTable: "user_files",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_user_file_comments_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "forum_topics",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					forum_id = table.Column<int>(type: "integer", nullable: false),
					title = table.Column<string>(type: "citext", maxLength: 500, nullable: false),
					poster_id = table.Column<int>(type: "integer", nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					is_locked = table.Column<bool>(type: "boolean", nullable: false),
					poll_id = table.Column<int>(type: "integer", nullable: true),
					submission_id = table.Column<int>(type: "integer", nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_topics", x => x.id);
					table.ForeignKey(
						name: "fk_forum_topics_forum_polls_poll_id",
						column: x => x.poll_id,
						principalTable: "forum_polls",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_forum_topics_forums_forum_id",
						column: x => x.forum_id,
						principalTable: "forums",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_forum_topics_submissions_submission_id1",
						column: x => x.submission_id,
						principalTable: "submissions",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_forum_topics_users_poster_id",
						column: x => x.poster_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publications",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					obsoleted_by_id = table.Column<int>(type: "integer", nullable: true),
					game_id = table.Column<int>(type: "integer", nullable: false),
					system_id = table.Column<int>(type: "integer", nullable: false),
					system_frame_rate_id = table.Column<int>(type: "integer", nullable: false),
					rom_id = table.Column<int>(type: "integer", nullable: false),
					publication_class_id = table.Column<int>(type: "integer", nullable: false),
					submission_id = table.Column<int>(type: "integer", nullable: false),
					wiki_content_id = table.Column<int>(type: "integer", nullable: true),
					movie_file = table.Column<byte[]>(type: "bytea", nullable: false),
					movie_file_name = table.Column<string>(type: "citext", nullable: false),
					branch = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					emulator_version = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					frames = table.Column<int>(type: "integer", nullable: false),
					rerecord_count = table.Column<int>(type: "integer", nullable: false),
					additional_authors = table.Column<string>(type: "citext", maxLength: 200, nullable: true),
					title = table.Column<string>(type: "citext", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publications", x => x.id);
					table.ForeignKey(
						name: "fk_publications_game_roms_rom_id",
						column: x => x.rom_id,
						principalTable: "game_roms",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_publications_game_system_frame_rates_system_frame_rate_id",
						column: x => x.system_frame_rate_id,
						principalTable: "game_system_frame_rates",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publications_game_systems_system_id",
						column: x => x.system_id,
						principalTable: "game_systems",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_publications_games_game_id",
						column: x => x.game_id,
						principalTable: "games",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publications_publication_classes_publication_class_id",
						column: x => x.publication_class_id,
						principalTable: "publication_classes",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publications_publications_obsoleted_by_id",
						column: x => x.obsoleted_by_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_publications_submissions_submission_id",
						column: x => x.submission_id,
						principalTable: "submissions",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publications_wiki_pages_wiki_content_id",
						column: x => x.wiki_content_id,
						principalTable: "wiki_pages",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "submission_authors",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					submission_id = table.Column<int>(type: "integer", nullable: false),
					ordinal = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_submission_authors", x => new { x.user_id, x.submission_id });
					table.ForeignKey(
						name: "fk_submission_authors_submissions_submission_id",
						column: x => x.submission_id,
						principalTable: "submissions",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_submission_authors_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "submission_status_history",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					submission_id = table.Column<int>(type: "integer", nullable: false),
					status = table.Column<int>(type: "integer", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_submission_status_history", x => x.id);
					table.ForeignKey(
						name: "fk_submission_status_history_submissions_submission_id",
						column: x => x.submission_id,
						principalTable: "submissions",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "forum_posts",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					topic_id = table.Column<int>(type: "integer", nullable: true),
					forum_id = table.Column<int>(type: "integer", nullable: false),
					poster_id = table.Column<int>(type: "integer", nullable: false),
					ip_address = table.Column<string>(type: "citext", maxLength: 50, nullable: true),
					subject = table.Column<string>(type: "citext", maxLength: 500, nullable: true),
					text = table.Column<string>(type: "citext", nullable: false),
					enable_html = table.Column<bool>(type: "boolean", nullable: false),
					enable_bb_code = table.Column<bool>(type: "boolean", nullable: false),
					poster_mood = table.Column<int>(type: "integer", nullable: false),
					search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
						.Annotation("Npgsql:TsVectorConfig", "english")
						.Annotation("Npgsql:TsVectorProperties", new[] { "text" }),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_posts", x => x.id);
					table.ForeignKey(
						name: "fk_forum_posts_forum_topics_topic_id",
						column: x => x.topic_id,
						principalTable: "forum_topics",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "fk_forum_posts_forums_forum_id",
						column: x => x.forum_id,
						principalTable: "forums",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_forum_posts_users_poster_id",
						column: x => x.poster_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "forum_topic_watches",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					forum_topic_id = table.Column<int>(type: "integer", nullable: false),
					is_notified = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_forum_topic_watches", x => new { x.user_id, x.forum_topic_id });
					table.ForeignKey(
						name: "fk_forum_topic_watches_forum_topics_forum_topic_id",
						column: x => x.forum_topic_id,
						principalTable: "forum_topics",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_forum_topic_watches_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "publication_authors",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					ordinal = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_authors", x => new { x.user_id, x.publication_id });
					table.ForeignKey(
						name: "fk_publication_authors_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_authors_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_awards",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					award_id = table.Column<int>(type: "integer", nullable: false),
					year = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_awards", x => x.id);
					table.ForeignKey(
						name: "fk_publication_awards_awards_award_id",
						column: x => x.award_id,
						principalTable: "awards",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_awards_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_files",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					path = table.Column<string>(type: "citext", maxLength: 250, nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					description = table.Column<string>(type: "citext", maxLength: 250, nullable: true),
					file_data = table.Column<byte[]>(type: "bytea", nullable: true),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_files", x => x.id);
					table.ForeignKey(
						name: "fk_publication_files_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_flags",
				columns: table => new
				{
					publication_id = table.Column<int>(type: "integer", nullable: false),
					flag_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_flags", x => new { x.publication_id, x.flag_id });
					table.ForeignKey(
						name: "fk_publication_flags_flags_flag_id",
						column: x => x.flag_id,
						principalTable: "flags",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_flags_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_maintenance_logs",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					log = table.Column<string>(type: "citext", nullable: false),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					user_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_maintenance_logs", x => x.id);
					table.ForeignKey(
						name: "fk_publication_maintenance_logs_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_maintenance_logs_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_ratings",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					value = table.Column<double>(type: "double precision", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_ratings", x => new { x.user_id, x.publication_id, x.type });
					table.ForeignKey(
						name: "fk_publication_ratings_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_ratings_users_user_id",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_tags",
				columns: table => new
				{
					publication_id = table.Column<int>(type: "integer", nullable: false),
					tag_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_tags", x => new { x.publication_id, x.tag_id });
					table.ForeignKey(
						name: "fk_publication_tags_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_publication_tags_tags_tag_id",
						column: x => x.tag_id,
						principalTable: "tags",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "publication_urls",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					publication_id = table.Column<int>(type: "integer", nullable: false),
					url = table.Column<string>(type: "citext", maxLength: 500, nullable: false),
					type = table.Column<int>(type: "integer", nullable: false),
					create_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					create_user_name = table.Column<string>(type: "citext", nullable: true),
					last_update_timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					last_update_user_name = table.Column<string>(type: "citext", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_publication_urls", x => x.id);
					table.ForeignKey(
						name: "fk_publication_urls_publications_publication_id",
						column: x => x.publication_id,
						principalTable: "publications",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "ix_deprecated_movie_formats_file_extension",
				table: "deprecated_movie_formats",
				column: "file_extension",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_flags_token",
				table: "flags",
				column: "token",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_forum_poll_option_votes_poll_option_id",
				table: "forum_poll_option_votes",
				column: "poll_option_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_poll_option_votes_user_id",
				table: "forum_poll_option_votes",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_poll_options_poll_id",
				table: "forum_poll_options",
				column: "poll_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_posts_forum_id",
				table: "forum_posts",
				column: "forum_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_posts_poster_id",
				table: "forum_posts",
				column: "poster_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_posts_search_vector",
				table: "forum_posts",
				column: "search_vector")
				.Annotation("Npgsql:IndexMethod", "GIN");

			migrationBuilder.CreateIndex(
				name: "ix_forum_posts_text",
				table: "forum_posts",
				column: "text")
				.Annotation("Npgsql:IndexMethod", "gin")
				.Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

			migrationBuilder.CreateIndex(
				name: "ix_forum_posts_topic_id",
				table: "forum_posts",
				column: "topic_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_topic_watches_forum_topic_id",
				table: "forum_topic_watches",
				column: "forum_topic_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_topics_forum_id",
				table: "forum_topics",
				column: "forum_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_topics_poll_id",
				table: "forum_topics",
				column: "poll_id",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_forum_topics_poster_id",
				table: "forum_topics",
				column: "poster_id");

			migrationBuilder.CreateIndex(
				name: "ix_forum_topics_submission_id",
				table: "forum_topics",
				column: "submission_id",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_forums_category_id",
				table: "forums",
				column: "category_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_game_groups_game_group_id",
				table: "game_game_groups",
				column: "game_group_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_game_groups_game_id",
				table: "game_game_groups",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_genres_game_id",
				table: "game_genres",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_genres_genre_id",
				table: "game_genres",
				column: "genre_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_groups_name",
				table: "game_groups",
				column: "name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_game_ram_address_domains_game_system_id",
				table: "game_ram_address_domains",
				column: "game_system_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_ram_addresses_game_id",
				table: "game_ram_addresses",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_ram_addresses_game_ram_address_domain_id",
				table: "game_ram_addresses",
				column: "game_ram_address_domain_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_ram_addresses_system_id",
				table: "game_ram_addresses",
				column: "system_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_roms_game_id",
				table: "game_roms",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_roms_md5",
				table: "game_roms",
				column: "md5",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_game_roms_sha1",
				table: "game_roms",
				column: "sha1",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_game_system_frame_rates_game_system_id",
				table: "game_system_frame_rates",
				column: "game_system_id");

			migrationBuilder.CreateIndex(
				name: "ix_game_systems_code",
				table: "game_systems",
				column: "code",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_games_system_id",
				table: "games",
				column: "system_id");

			migrationBuilder.CreateIndex(
				name: "ix_ip_bans_mask",
				table: "ip_bans",
				column: "mask",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_private_messages_from_user_id",
				table: "private_messages",
				column: "from_user_id");

			migrationBuilder.CreateIndex(
				name: "ix_private_messages_to_user_id_read_on_deleted_for_to_user",
				table: "private_messages",
				columns: new[] { "to_user_id", "read_on", "deleted_for_to_user" });

			migrationBuilder.CreateIndex(
				name: "ix_publication_authors_publication_id",
				table: "publication_authors",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_awards_award_id",
				table: "publication_awards",
				column: "award_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_awards_publication_id",
				table: "publication_awards",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_classes_name",
				table: "publication_classes",
				column: "name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_publication_files_publication_id",
				table: "publication_files",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_flags_flag_id",
				table: "publication_flags",
				column: "flag_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_flags_publication_id",
				table: "publication_flags",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_maintenance_logs_publication_id",
				table: "publication_maintenance_logs",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_maintenance_logs_user_id",
				table: "publication_maintenance_logs",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_ratings_publication_id",
				table: "publication_ratings",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_ratings_user_id_publication_id_type",
				table: "publication_ratings",
				columns: new[] { "user_id", "publication_id", "type" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_publication_tags_publication_id",
				table: "publication_tags",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_tags_tag_id",
				table: "publication_tags",
				column: "tag_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_urls_publication_id",
				table: "publication_urls",
				column: "publication_id");

			migrationBuilder.CreateIndex(
				name: "ix_publication_urls_type",
				table: "publication_urls",
				column: "type");

			migrationBuilder.CreateIndex(
				name: "ix_publications_game_id",
				table: "publications",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_obsoleted_by_id",
				table: "publications",
				column: "obsoleted_by_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_publication_class_id",
				table: "publications",
				column: "publication_class_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_rom_id",
				table: "publications",
				column: "rom_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_submission_id",
				table: "publications",
				column: "submission_id",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_publications_system_frame_rate_id",
				table: "publications",
				column: "system_frame_rate_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_system_id",
				table: "publications",
				column: "system_id");

			migrationBuilder.CreateIndex(
				name: "ix_publications_wiki_content_id",
				table: "publications",
				column: "wiki_content_id");

			migrationBuilder.CreateIndex(
				name: "ix_role_claims_role_id",
				table: "role_claims",
				column: "role_id");

			migrationBuilder.CreateIndex(
				name: "ix_role_links_role_id",
				table: "role_links",
				column: "role_id");

			migrationBuilder.CreateIndex(
				name: "ix_submission_authors_submission_id",
				table: "submission_authors",
				column: "submission_id");

			migrationBuilder.CreateIndex(
				name: "ix_submission_rejection_reasons_display_name",
				table: "submission_rejection_reasons",
				column: "display_name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_submission_status_history_submission_id",
				table: "submission_status_history",
				column: "submission_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_game_id",
				table: "submissions",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_intended_class_id",
				table: "submissions",
				column: "intended_class_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_judge_id",
				table: "submissions",
				column: "judge_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_publisher_id",
				table: "submissions",
				column: "publisher_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_rejection_reason_id",
				table: "submissions",
				column: "rejection_reason_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_rom_id",
				table: "submissions",
				column: "rom_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_status",
				table: "submissions",
				column: "status");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_submitter_id",
				table: "submissions",
				column: "submitter_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_system_frame_rate_id",
				table: "submissions",
				column: "system_frame_rate_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_system_id",
				table: "submissions",
				column: "system_id");

			migrationBuilder.CreateIndex(
				name: "ix_submissions_wiki_content_id",
				table: "submissions",
				column: "wiki_content_id");

			migrationBuilder.CreateIndex(
				name: "ix_tags_code",
				table: "tags",
				column: "code",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_user_awards_award_id",
				table: "user_awards",
				column: "award_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_awards_user_id",
				table: "user_awards",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_claims_user_id",
				table: "user_claims",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_disallows_regex_pattern",
				table: "user_disallows",
				column: "regex_pattern",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_user_file_comments_parent_id",
				table: "user_file_comments",
				column: "parent_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_file_comments_user_file_id",
				table: "user_file_comments",
				column: "user_file_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_file_comments_user_id",
				table: "user_file_comments",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_files_author_id",
				table: "user_files",
				column: "author_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_files_game_id",
				table: "user_files",
				column: "game_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_files_hidden",
				table: "user_files",
				column: "hidden");

			migrationBuilder.CreateIndex(
				name: "ix_user_files_system_id",
				table: "user_files",
				column: "system_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_logins_user_id",
				table: "user_logins",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_maintenance_logs_editor_id",
				table: "user_maintenance_logs",
				column: "editor_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_maintenance_logs_user_id",
				table: "user_maintenance_logs",
				column: "user_id");

			migrationBuilder.CreateIndex(
				name: "ix_user_roles_role_id",
				table: "user_roles",
				column: "role_id");

			migrationBuilder.CreateIndex(
				name: "ix_users_normalized_user_name",
				table: "users",
				column: "normalized_user_name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_wiki_pages_author_id",
				table: "wiki_pages",
				column: "author_id");

			migrationBuilder.CreateIndex(
				name: "ix_wiki_pages_child_id",
				table: "wiki_pages",
				column: "child_id");

			migrationBuilder.CreateIndex(
				name: "ix_wiki_pages_markup",
				table: "wiki_pages",
				column: "markup")
				.Annotation("Npgsql:IndexMethod", "gin")
				.Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

			migrationBuilder.CreateIndex(
				name: "ix_wiki_pages_page_name_revision",
				table: "wiki_pages",
				columns: new[] { "page_name", "revision" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_wiki_pages_search_vector",
				table: "wiki_pages",
				column: "search_vector")
				.Annotation("Npgsql:IndexMethod", "GIN");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "deprecated_movie_formats");

			migrationBuilder.DropTable(
				name: "forum_poll_option_votes");

			migrationBuilder.DropTable(
				name: "forum_posts");

			migrationBuilder.DropTable(
				name: "forum_topic_watches");

			migrationBuilder.DropTable(
				name: "game_game_groups");

			migrationBuilder.DropTable(
				name: "game_genres");

			migrationBuilder.DropTable(
				name: "game_ram_addresses");

			migrationBuilder.DropTable(
				name: "ip_bans");

			migrationBuilder.DropTable(
				name: "media_posts");

			migrationBuilder.DropTable(
				name: "private_messages");

			migrationBuilder.DropTable(
				name: "publication_authors");

			migrationBuilder.DropTable(
				name: "publication_awards");

			migrationBuilder.DropTable(
				name: "publication_files");

			migrationBuilder.DropTable(
				name: "publication_flags");

			migrationBuilder.DropTable(
				name: "publication_maintenance_logs");

			migrationBuilder.DropTable(
				name: "publication_ratings");

			migrationBuilder.DropTable(
				name: "publication_tags");

			migrationBuilder.DropTable(
				name: "publication_urls");

			migrationBuilder.DropTable(
				name: "role_claims");

			migrationBuilder.DropTable(
				name: "role_links");

			migrationBuilder.DropTable(
				name: "role_permission");

			migrationBuilder.DropTable(
				name: "submission_authors");

			migrationBuilder.DropTable(
				name: "submission_status_history");

			migrationBuilder.DropTable(
				name: "user_awards");

			migrationBuilder.DropTable(
				name: "user_claims");

			migrationBuilder.DropTable(
				name: "user_disallows");

			migrationBuilder.DropTable(
				name: "user_file_comments");

			migrationBuilder.DropTable(
				name: "user_logins");

			migrationBuilder.DropTable(
				name: "user_maintenance_logs");

			migrationBuilder.DropTable(
				name: "user_roles");

			migrationBuilder.DropTable(
				name: "user_tokens");

			migrationBuilder.DropTable(
				name: "wiki_referrals");

			migrationBuilder.DropTable(
				name: "forum_poll_options");

			migrationBuilder.DropTable(
				name: "forum_topics");

			migrationBuilder.DropTable(
				name: "game_groups");

			migrationBuilder.DropTable(
				name: "genres");

			migrationBuilder.DropTable(
				name: "game_ram_address_domains");

			migrationBuilder.DropTable(
				name: "flags");

			migrationBuilder.DropTable(
				name: "tags");

			migrationBuilder.DropTable(
				name: "publications");

			migrationBuilder.DropTable(
				name: "awards");

			migrationBuilder.DropTable(
				name: "user_files");

			migrationBuilder.DropTable(
				name: "roles");

			migrationBuilder.DropTable(
				name: "forum_polls");

			migrationBuilder.DropTable(
				name: "forums");

			migrationBuilder.DropTable(
				name: "submissions");

			migrationBuilder.DropTable(
				name: "forum_categories");

			migrationBuilder.DropTable(
				name: "game_roms");

			migrationBuilder.DropTable(
				name: "game_system_frame_rates");

			migrationBuilder.DropTable(
				name: "publication_classes");

			migrationBuilder.DropTable(
				name: "submission_rejection_reasons");

			migrationBuilder.DropTable(
				name: "wiki_pages");

			migrationBuilder.DropTable(
				name: "games");

			migrationBuilder.DropTable(
				name: "users");

			migrationBuilder.DropTable(
				name: "game_systems");
		}
	}
}
