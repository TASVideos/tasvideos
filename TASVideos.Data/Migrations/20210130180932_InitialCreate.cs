using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TASVideos.Data.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Awards",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Type = table.Column<int>(type: "int", nullable: false),
					ShortName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
					Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Awards", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Flags",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					IconPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LinkPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
					PermissionRestriction = table.Column<int>(type: "int", nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Flags", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ForumCategories",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Title = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
					Ordinal = table.Column<int>(type: "int", nullable: false),
					Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumCategories", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ForumPolls",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					TopicId = table.Column<int>(type: "int", nullable: false),
					Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
					CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumPolls", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "GameGroups",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					SearchKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameGroups", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "GameSystems",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
					DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameSystems", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Genres",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					DisplayName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Genres", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "MediaPosts",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
					Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					Body = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
					Group = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MediaPosts", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "RoleClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					RoleId = table.Column<int>(type: "int", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RoleClaims", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Roles",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					IsDefault = table.Column<bool>(type: "bit", nullable: false),
					Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
					AutoAssignPostCount = table.Column<int>(type: "int", nullable: true),
					AutoAssignPublications = table.Column<bool>(type: "bit", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
					NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Roles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "SubmissionRejectionReasons",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SubmissionRejectionReasons", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tags",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Code = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
					DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tags", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tiers",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
					Weight = table.Column<double>(type: "float", nullable: false),
					IconPath = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					Link = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tiers", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "User",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					LastLoggedInTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
					TimeZoneId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Avatar = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
					From = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					Signature = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
					PublicRatings = table.Column<bool>(type: "bit", nullable: false),
					MoodAvatarUrlBase = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
					UseRatings = table.Column<bool>(type: "bit", nullable: false),
					LegacyPassword = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
					UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					NormalizedUserName = table.Column<string>(type: "nvarchar(450)", nullable: true),
					Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
					NormalizedEmail = table.Column<string>(type: "nvarchar(450)", nullable: true),
					EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
					PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
					SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
					TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
					LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
					LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
					AccessFailedCount = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_User", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "UserClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<int>(type: "int", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserClaims", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "UserDisallows",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					RegexPattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserDisallows", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "UserLogins",
				columns: table => new
				{
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					UserId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
				});

			migrationBuilder.CreateTable(
				name: "UserTokens",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
				});

			migrationBuilder.CreateTable(
				name: "WikiPages",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					PageName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					Markup = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Revision = table.Column<int>(type: "int", nullable: false),
					MinorEdit = table.Column<bool>(type: "bit", nullable: false),
					RevisionMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
					ChildId = table.Column<int>(type: "int", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_WikiPages", x => x.Id);
					table.ForeignKey(
						name: "FK_WikiPages_WikiPages_ChildId",
						column: x => x.ChildId,
						principalTable: "WikiPages",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "WikiReferrals",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Referrer = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					Referral = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
					Excerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_WikiReferrals", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Forums",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					CategoryId = table.Column<int>(type: "int", nullable: false),
					Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
					Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
					Ordinal = table.Column<int>(type: "int", nullable: false),
					Restricted = table.Column<bool>(type: "bit", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Forums", x => x.Id);
					table.ForeignKey(
						name: "FK_Forums_ForumCategories_CategoryId",
						column: x => x.CategoryId,
						principalTable: "ForumCategories",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ForumPollOptions",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Text = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					Ordinal = table.Column<int>(type: "int", nullable: false),
					PollId = table.Column<int>(type: "int", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumPollOptions", x => x.Id);
					table.ForeignKey(
						name: "FK_ForumPollOptions_ForumPolls_PollId",
						column: x => x.PollId,
						principalTable: "ForumPolls",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameRamAddressDomains",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					GameSystemId = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameRamAddressDomains", x => x.Id);
					table.ForeignKey(
						name: "FK_GameRamAddressDomains_GameSystems_GameSystemId",
						column: x => x.GameSystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Games",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					SystemId = table.Column<int>(type: "int", nullable: false),
					GoodName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					Abbreviation = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
					SearchKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
					YoutubeTags = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					ScreenshotUrl = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
					GameResourcesPage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Games", x => x.Id);
					table.ForeignKey(
						name: "FK_Games_GameSystems_SystemId",
						column: x => x.SystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameSystemFrameRates",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					GameSystemId = table.Column<int>(type: "int", nullable: false),
					FrameRate = table.Column<double>(type: "float", nullable: false),
					RegionCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
					Preliminary = table.Column<bool>(type: "bit", nullable: false),
					Obsolete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameSystemFrameRates", x => x.Id);
					table.ForeignKey(
						name: "FK_GameSystemFrameRates_GameSystems_GameSystemId",
						column: x => x.GameSystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "RoleLinks",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Link = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
					RoleId = table.Column<int>(type: "int", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RoleLinks", x => x.Id);
					table.ForeignKey(
						name: "FK_RoleLinks_Roles_RoleId",
						column: x => x.RoleId,
						principalTable: "Roles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "RolePermission",
				columns: table => new
				{
					RoleId = table.Column<int>(type: "int", nullable: false),
					PermissionId = table.Column<int>(type: "int", nullable: false),
					CanAssign = table.Column<bool>(type: "bit", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
					table.ForeignKey(
						name: "FK_RolePermission_Roles_RoleId",
						column: x => x.RoleId,
						principalTable: "Roles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PrivateMessages",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					FromUserId = table.Column<int>(type: "int", nullable: false),
					ToUserId = table.Column<int>(type: "int", nullable: false),
					IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
					Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
					EnableHtml = table.Column<bool>(type: "bit", nullable: false),
					EnableBbCode = table.Column<bool>(type: "bit", nullable: false),
					ReadOn = table.Column<DateTime>(type: "datetime2", nullable: true),
					SavedForFromUser = table.Column<bool>(type: "bit", nullable: false),
					SavedForToUser = table.Column<bool>(type: "bit", nullable: false),
					DeletedForFromUser = table.Column<bool>(type: "bit", nullable: false),
					DeletedForToUser = table.Column<bool>(type: "bit", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PrivateMessages", x => x.Id);
					table.ForeignKey(
						name: "FK_PrivateMessages_User_FromUserId",
						column: x => x.FromUserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_PrivateMessages_User_ToUserId",
						column: x => x.ToUserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "UserAwards",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<int>(type: "int", nullable: false),
					AwardId = table.Column<int>(type: "int", nullable: false),
					Year = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserAwards", x => x.Id);
					table.ForeignKey(
						name: "FK_UserAwards_Awards_AwardId",
						column: x => x.AwardId,
						principalTable: "Awards",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_UserAwards_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "UserRoles",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					RoleId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
					table.ForeignKey(
						name: "FK_UserRoles_Roles_RoleId",
						column: x => x.RoleId,
						principalTable: "Roles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_UserRoles_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ForumTopics",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ForumId = table.Column<int>(type: "int", nullable: false),
					Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
					PosterId = table.Column<int>(type: "int", nullable: false),
					Views = table.Column<int>(type: "int", nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					IsLocked = table.Column<bool>(type: "bit", nullable: false),
					PollId = table.Column<int>(type: "int", nullable: true),
					PageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumTopics", x => x.Id);
					table.ForeignKey(
						name: "FK_ForumTopics_ForumPolls_PollId",
						column: x => x.PollId,
						principalTable: "ForumPolls",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_ForumTopics_Forums_ForumId",
						column: x => x.ForumId,
						principalTable: "Forums",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ForumTopics_User_PosterId",
						column: x => x.PosterId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ForumPollOptionVotes",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					PollOptionId = table.Column<int>(type: "int", nullable: false),
					UserId = table.Column<int>(type: "int", nullable: false),
					CreateTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumPollOptionVotes", x => x.Id);
					table.ForeignKey(
						name: "FK_ForumPollOptionVotes_ForumPollOptions_PollOptionId",
						column: x => x.PollOptionId,
						principalTable: "ForumPollOptions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ForumPollOptionVotes_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameGameGroups",
				columns: table => new
				{
					GameId = table.Column<int>(type: "int", nullable: false),
					GameGroupId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameGameGroups", x => new { x.GameId, x.GameGroupId });
					table.ForeignKey(
						name: "FK_GameGameGroups_GameGroups_GameGroupId",
						column: x => x.GameGroupId,
						principalTable: "GameGroups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_GameGameGroups_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameGenres",
				columns: table => new
				{
					GameId = table.Column<int>(type: "int", nullable: false),
					GenreId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameGenres", x => new { x.GameId, x.GenreId });
					table.ForeignKey(
						name: "FK_GameGenres_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_GameGenres_Genres_GenreId",
						column: x => x.GenreId,
						principalTable: "Genres",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameRamAddresses",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Address = table.Column<long>(type: "bigint", nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					Signed = table.Column<int>(type: "int", nullable: false),
					Endian = table.Column<int>(type: "int", nullable: false),
					Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					GameRamAddressDomainId = table.Column<int>(type: "int", nullable: false),
					GameId = table.Column<int>(type: "int", nullable: true),
					LegacyGameName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
					SystemId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameRamAddresses", x => x.Id);
					table.ForeignKey(
						name: "FK_GameRamAddresses_GameRamAddressDomains_GameRamAddressDomainId",
						column: x => x.GameRamAddressDomainId,
						principalTable: "GameRamAddressDomains",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_GameRamAddresses_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_GameRamAddresses_GameSystems_SystemId",
						column: x => x.SystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GameRoms",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					GameId = table.Column<int>(type: "int", nullable: false),
					Md5 = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
					Sha1 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
					Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GameRoms", x => x.Id);
					table.ForeignKey(
						name: "FK_GameRoms_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "UserFiles",
				columns: table => new
				{
					Id = table.Column<long>(type: "bigint", nullable: false),
					AuthorId = table.Column<int>(type: "int", nullable: false),
					FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
					Class = table.Column<int>(type: "int", nullable: false),
					Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
					UploadTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					Length = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
					Frames = table.Column<int>(type: "int", nullable: false),
					Rerecords = table.Column<int>(type: "int", nullable: false),
					Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LogicalLength = table.Column<int>(type: "int", nullable: false),
					PhysicalLength = table.Column<int>(type: "int", nullable: false),
					GameId = table.Column<int>(type: "int", nullable: true),
					SystemId = table.Column<int>(type: "int", nullable: true),
					Hidden = table.Column<bool>(type: "bit", nullable: false),
					Warnings = table.Column<string>(type: "nvarchar(max)", nullable: true),
					Views = table.Column<int>(type: "int", nullable: false),
					Downloads = table.Column<int>(type: "int", nullable: false),
					CompressionType = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_UserFiles_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_UserFiles_GameSystems_SystemId",
						column: x => x.SystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_UserFiles_User_AuthorId",
						column: x => x.AuthorId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "ForumPosts",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					TopicId = table.Column<int>(type: "int", nullable: true),
					PosterId = table.Column<int>(type: "int", nullable: false),
					IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
					Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
					EnableHtml = table.Column<bool>(type: "bit", nullable: false),
					EnableBbCode = table.Column<bool>(type: "bit", nullable: false),
					PosterMood = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumPosts", x => x.Id);
					table.ForeignKey(
						name: "FK_ForumPosts_ForumTopics_TopicId",
						column: x => x.TopicId,
						principalTable: "ForumTopics",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_ForumPosts_User_PosterId",
						column: x => x.PosterId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ForumTopicWatches",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					ForumTopicId = table.Column<int>(type: "int", nullable: false),
					IsNotified = table.Column<bool>(type: "bit", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ForumTopicWatches", x => new { x.UserId, x.ForumTopicId });
					table.ForeignKey(
						name: "FK_ForumTopicWatches_ForumTopics_ForumTopicId",
						column: x => x.ForumTopicId,
						principalTable: "ForumTopics",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ForumTopicWatches_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "Submissions",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					WikiContentId = table.Column<int>(type: "int", nullable: true),
					SubmitterId = table.Column<int>(type: "int", nullable: true),
					IntendedTierId = table.Column<int>(type: "int", nullable: true),
					JudgeId = table.Column<int>(type: "int", nullable: true),
					PublisherId = table.Column<int>(type: "int", nullable: true),
					Status = table.Column<int>(type: "int", nullable: false),
					MovieFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
					MovieExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
					GameId = table.Column<int>(type: "int", nullable: true),
					RomId = table.Column<int>(type: "int", nullable: true),
					SystemId = table.Column<int>(type: "int", nullable: true),
					SystemFrameRateId = table.Column<int>(type: "int", nullable: true),
					Frames = table.Column<int>(type: "int", nullable: false),
					RerecordCount = table.Column<int>(type: "int", nullable: false),
					EncodeEmbedLink = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					GameVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					GameName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					Branch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					RomName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
					EmulatorVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					MovieStartType = table.Column<int>(type: "int", nullable: true),
					RejectionReasonId = table.Column<int>(type: "int", nullable: true),
					AdditionalAuthors = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
					LegacyTime = table.Column<decimal>(type: "decimal(16,4)", nullable: false, defaultValue: 0m),
					ImportedTime = table.Column<decimal>(type: "decimal(16,4)", nullable: false, defaultValue: 0m),
					LegacyAlerts = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Submissions", x => x.Id);
					table.ForeignKey(
						name: "FK_Submissions_GameRoms_RomId",
						column: x => x.RomId,
						principalTable: "GameRoms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_GameSystemFrameRates_SystemFrameRateId",
						column: x => x.SystemFrameRateId,
						principalTable: "GameSystemFrameRates",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_GameSystems_SystemId",
						column: x => x.SystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_SubmissionRejectionReasons_RejectionReasonId",
						column: x => x.RejectionReasonId,
						principalTable: "SubmissionRejectionReasons",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_Tiers_IntendedTierId",
						column: x => x.IntendedTierId,
						principalTable: "Tiers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_User_JudgeId",
						column: x => x.JudgeId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_User_PublisherId",
						column: x => x.PublisherId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_User_SubmitterId",
						column: x => x.SubmitterId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Submissions_WikiPages_WikiContentId",
						column: x => x.WikiContentId,
						principalTable: "WikiPages",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "UserFileComments",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserFileId = table.Column<long>(type: "bigint", nullable: false),
					Ip = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
					ParentId = table.Column<int>(type: "int", nullable: true),
					Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
					Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreationTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					UserId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserFileComments", x => x.Id);
					table.ForeignKey(
						name: "FK_UserFileComments_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_UserFileComments_UserFileComments_ParentId",
						column: x => x.ParentId,
						principalTable: "UserFileComments",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_UserFileComments_UserFiles_UserFileId",
						column: x => x.UserFileId,
						principalTable: "UserFiles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Publications",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					ObsoletedById = table.Column<int>(type: "int", nullable: true),
					GameId = table.Column<int>(type: "int", nullable: false),
					SystemId = table.Column<int>(type: "int", nullable: false),
					SystemFrameRateId = table.Column<int>(type: "int", nullable: false),
					RomId = table.Column<int>(type: "int", nullable: false),
					TierId = table.Column<int>(type: "int", nullable: false),
					SubmissionId = table.Column<int>(type: "int", nullable: false),
					WikiContentId = table.Column<int>(type: "int", nullable: true),
					MovieFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
					MovieFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Branch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					EmulatorVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
					Frames = table.Column<int>(type: "int", nullable: false),
					RerecordCount = table.Column<int>(type: "int", nullable: false),
					AdditionalAuthors = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
					Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Publications", x => x.Id);
					table.ForeignKey(
						name: "FK_Publications_GameRoms_RomId",
						column: x => x.RomId,
						principalTable: "GameRoms",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Publications_Games_GameId",
						column: x => x.GameId,
						principalTable: "Games",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Publications_GameSystemFrameRates_SystemFrameRateId",
						column: x => x.SystemFrameRateId,
						principalTable: "GameSystemFrameRates",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Publications_GameSystems_SystemId",
						column: x => x.SystemId,
						principalTable: "GameSystems",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Publications_Publications_ObsoletedById",
						column: x => x.ObsoletedById,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Publications_Submissions_SubmissionId",
						column: x => x.SubmissionId,
						principalTable: "Submissions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Publications_Tiers_TierId",
						column: x => x.TierId,
						principalTable: "Tiers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Publications_WikiPages_WikiContentId",
						column: x => x.WikiContentId,
						principalTable: "WikiPages",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "SubmissionAuthors",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					SubmissionId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SubmissionAuthors", x => new { x.UserId, x.SubmissionId });
					table.ForeignKey(
						name: "FK_SubmissionAuthors_Submissions_SubmissionId",
						column: x => x.SubmissionId,
						principalTable: "Submissions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_SubmissionAuthors_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "SubmissionStatusHistory",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					SubmissionId = table.Column<int>(type: "int", nullable: false),
					Status = table.Column<int>(type: "int", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SubmissionStatusHistory", x => x.Id);
					table.ForeignKey(
						name: "FK_SubmissionStatusHistory_Submissions_SubmissionId",
						column: x => x.SubmissionId,
						principalTable: "Submissions",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationAuthors",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					PublicationId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationAuthors", x => new { x.UserId, x.PublicationId });
					table.ForeignKey(
						name: "FK_PublicationAuthors_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PublicationAuthors_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationAwards",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					PublicationId = table.Column<int>(type: "int", nullable: false),
					AwardId = table.Column<int>(type: "int", nullable: false),
					Year = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationAwards", x => x.Id);
					table.ForeignKey(
						name: "FK_PublicationAwards_Awards_AwardId",
						column: x => x.AwardId,
						principalTable: "Awards",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PublicationAwards_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationFiles",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					PublicationId = table.Column<int>(type: "int", nullable: false),
					Path = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
					FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_PublicationFiles_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationFlags",
				columns: table => new
				{
					PublicationId = table.Column<int>(type: "int", nullable: false),
					FlagId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationFlags", x => new { x.PublicationId, x.FlagId });
					table.ForeignKey(
						name: "FK_PublicationFlags_Flags_FlagId",
						column: x => x.FlagId,
						principalTable: "Flags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PublicationFlags_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationRatings",
				columns: table => new
				{
					UserId = table.Column<int>(type: "int", nullable: false),
					PublicationId = table.Column<int>(type: "int", nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					Value = table.Column<double>(type: "float", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationRatings", x => new { x.UserId, x.PublicationId, x.Type });
					table.ForeignKey(
						name: "FK_PublicationRatings_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PublicationRatings_User_UserId",
						column: x => x.UserId,
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationTags",
				columns: table => new
				{
					PublicationId = table.Column<int>(type: "int", nullable: false),
					TagId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationTags", x => new { x.PublicationId, x.TagId });
					table.ForeignKey(
						name: "FK_PublicationTags_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PublicationTags_Tags_TagId",
						column: x => x.TagId,
						principalTable: "Tags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PublicationUrls",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					PublicationId = table.Column<int>(type: "int", nullable: false),
					Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
					Type = table.Column<int>(type: "int", nullable: false),
					CreateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LastUpdateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					LastUpdateUserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PublicationUrls", x => x.Id);
					table.ForeignKey(
						name: "FK_PublicationUrls_Publications_PublicationId",
						column: x => x.PublicationId,
						principalTable: "Publications",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ForumPollOptions_PollId",
				table: "ForumPollOptions",
				column: "PollId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumPollOptionVotes_PollOptionId",
				table: "ForumPollOptionVotes",
				column: "PollOptionId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumPollOptionVotes_UserId",
				table: "ForumPollOptionVotes",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumPosts_PosterId",
				table: "ForumPosts",
				column: "PosterId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumPosts_TopicId",
				table: "ForumPosts",
				column: "TopicId");

			migrationBuilder.CreateIndex(
				name: "IX_Forums_CategoryId",
				table: "Forums",
				column: "CategoryId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumTopics_ForumId",
				table: "ForumTopics",
				column: "ForumId");

			migrationBuilder.CreateIndex(
				name: "IX_ForumTopics_PollId",
				table: "ForumTopics",
				column: "PollId",
				unique: true,
				filter: "[PollId] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_ForumTopics_PosterId",
				table: "ForumTopics",
				column: "PosterId");

			migrationBuilder.CreateIndex(
				name: "PageNameIndex",
				table: "ForumTopics",
				column: "PageName",
				unique: true,
				filter: "([PageName] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_ForumTopicWatches_ForumTopicId",
				table: "ForumTopicWatches",
				column: "ForumTopicId");

			migrationBuilder.CreateIndex(
				name: "IX_GameGameGroups_GameGroupId",
				table: "GameGameGroups",
				column: "GameGroupId");

			migrationBuilder.CreateIndex(
				name: "IX_GameGameGroups_GameId",
				table: "GameGameGroups",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_GameGenres_GameId",
				table: "GameGenres",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_GameGenres_GenreId",
				table: "GameGenres",
				column: "GenreId");

			migrationBuilder.CreateIndex(
				name: "IX_GameGroups_Name",
				table: "GameGroups",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_GameRamAddressDomains_GameSystemId",
				table: "GameRamAddressDomains",
				column: "GameSystemId");

			migrationBuilder.CreateIndex(
				name: "IX_GameRamAddresses_GameId",
				table: "GameRamAddresses",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_GameRamAddresses_GameRamAddressDomainId",
				table: "GameRamAddresses",
				column: "GameRamAddressDomainId");

			migrationBuilder.CreateIndex(
				name: "IX_GameRamAddresses_SystemId",
				table: "GameRamAddresses",
				column: "SystemId");

			migrationBuilder.CreateIndex(
				name: "IX_GameRoms_GameId",
				table: "GameRoms",
				column: "GameId");

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
				name: "IX_Games_SystemId",
				table: "Games",
				column: "SystemId");

			migrationBuilder.CreateIndex(
				name: "IX_GameSystemFrameRates_GameSystemId",
				table: "GameSystemFrameRates",
				column: "GameSystemId");

			migrationBuilder.CreateIndex(
				name: "IX_GameSystems_Code",
				table: "GameSystems",
				column: "Code",
				unique: true,
				filter: "([Code] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_PrivateMessages_FromUserId",
				table: "PrivateMessages",
				column: "FromUserId");

			migrationBuilder.CreateIndex(
				name: "IX_PrivateMessages_ToUserId_ReadOn_DeletedForToUser",
				table: "PrivateMessages",
				columns: new[] { "ToUserId", "ReadOn", "DeletedForToUser" });

			migrationBuilder.CreateIndex(
				name: "IX_PublicationAuthors_PublicationId",
				table: "PublicationAuthors",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationAwards_AwardId",
				table: "PublicationAwards",
				column: "AwardId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationAwards_PublicationId",
				table: "PublicationAwards",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationFiles_PublicationId",
				table: "PublicationFiles",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationFlags_FlagId",
				table: "PublicationFlags",
				column: "FlagId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationFlags_PublicationId",
				table: "PublicationFlags",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationRatings_PublicationId",
				table: "PublicationRatings",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationRatings_UserId_PublicationId_Type",
				table: "PublicationRatings",
				columns: new[] { "UserId", "PublicationId", "Type" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Publications_GameId",
				table: "Publications",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_ObsoletedById",
				table: "Publications",
				column: "ObsoletedById");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_RomId",
				table: "Publications",
				column: "RomId");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_SubmissionId",
				table: "Publications",
				column: "SubmissionId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Publications_SystemFrameRateId",
				table: "Publications",
				column: "SystemFrameRateId");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_SystemId",
				table: "Publications",
				column: "SystemId");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_TierId",
				table: "Publications",
				column: "TierId");

			migrationBuilder.CreateIndex(
				name: "IX_Publications_WikiContentId",
				table: "Publications",
				column: "WikiContentId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationTags_PublicationId",
				table: "PublicationTags",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationTags_TagId",
				table: "PublicationTags",
				column: "TagId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationUrls_PublicationId",
				table: "PublicationUrls",
				column: "PublicationId");

			migrationBuilder.CreateIndex(
				name: "IX_PublicationUrls_Type",
				table: "PublicationUrls",
				column: "Type");

			migrationBuilder.CreateIndex(
				name: "IX_RoleClaims_RoleId",
				table: "RoleClaims",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "IX_RoleLinks_RoleId",
				table: "RoleLinks",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "IX_SubmissionAuthors_SubmissionId",
				table: "SubmissionAuthors",
				column: "SubmissionId");

			migrationBuilder.CreateIndex(
				name: "IX_SubmissionRejectionReasons_DisplayName",
				table: "SubmissionRejectionReasons",
				column: "DisplayName",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_GameId",
				table: "Submissions",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_IntendedTierId",
				table: "Submissions",
				column: "IntendedTierId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_JudgeId",
				table: "Submissions",
				column: "JudgeId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_PublisherId",
				table: "Submissions",
				column: "PublisherId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_RejectionReasonId",
				table: "Submissions",
				column: "RejectionReasonId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_RomId",
				table: "Submissions",
				column: "RomId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_Status",
				table: "Submissions",
				column: "Status");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_SubmitterId",
				table: "Submissions",
				column: "SubmitterId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_SystemFrameRateId",
				table: "Submissions",
				column: "SystemFrameRateId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_SystemId",
				table: "Submissions",
				column: "SystemId");

			migrationBuilder.CreateIndex(
				name: "IX_Submissions_WikiContentId",
				table: "Submissions",
				column: "WikiContentId");

			migrationBuilder.CreateIndex(
				name: "IX_SubmissionStatusHistory_SubmissionId",
				table: "SubmissionStatusHistory",
				column: "SubmissionId");

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Code",
				table: "Tags",
				column: "Code",
				unique: true,
				filter: "([Code] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_Tiers_Name",
				table: "Tiers",
				column: "Name",
				unique: true,
				filter: "([Name] IS NOT NULL)");

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
				name: "IX_UserAwards_AwardId",
				table: "UserAwards",
				column: "AwardId");

			migrationBuilder.CreateIndex(
				name: "IX_UserAwards_UserId",
				table: "UserAwards",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_UserClaims_UserId",
				table: "UserClaims",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "UserDisallowRegexPatternIndex",
				table: "UserDisallows",
				column: "RegexPattern",
				unique: true,
				filter: "([RegexPattern] IS NOT NULL)");

			migrationBuilder.CreateIndex(
				name: "IX_UserFileComments_ParentId",
				table: "UserFileComments",
				column: "ParentId");

			migrationBuilder.CreateIndex(
				name: "IX_UserFileComments_UserFileId",
				table: "UserFileComments",
				column: "UserFileId");

			migrationBuilder.CreateIndex(
				name: "IX_UserFileComments_UserId",
				table: "UserFileComments",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_UserFiles_AuthorId",
				table: "UserFiles",
				column: "AuthorId");

			migrationBuilder.CreateIndex(
				name: "IX_UserFiles_GameId",
				table: "UserFiles",
				column: "GameId");

			migrationBuilder.CreateIndex(
				name: "IX_UserFiles_Hidden",
				table: "UserFiles",
				column: "Hidden");

			migrationBuilder.CreateIndex(
				name: "IX_UserFiles_SystemId",
				table: "UserFiles",
				column: "SystemId");

			migrationBuilder.CreateIndex(
				name: "IX_UserLogins_UserId",
				table: "UserLogins",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_UserRoles_RoleId",
				table: "UserRoles",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "IX_WikiPages_ChildId",
				table: "WikiPages",
				column: "ChildId");

			migrationBuilder.CreateIndex(
				name: "PageNameIndex",
				table: "WikiPages",
				columns: new[] { "PageName", "Revision" },
				unique: true,
				filter: "([PageName] IS NOT NULL)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ForumPollOptionVotes");

			migrationBuilder.DropTable(
				name: "ForumPosts");

			migrationBuilder.DropTable(
				name: "ForumTopicWatches");

			migrationBuilder.DropTable(
				name: "GameGameGroups");

			migrationBuilder.DropTable(
				name: "GameGenres");

			migrationBuilder.DropTable(
				name: "GameRamAddresses");

			migrationBuilder.DropTable(
				name: "MediaPosts");

			migrationBuilder.DropTable(
				name: "PrivateMessages");

			migrationBuilder.DropTable(
				name: "PublicationAuthors");

			migrationBuilder.DropTable(
				name: "PublicationAwards");

			migrationBuilder.DropTable(
				name: "PublicationFiles");

			migrationBuilder.DropTable(
				name: "PublicationFlags");

			migrationBuilder.DropTable(
				name: "PublicationRatings");

			migrationBuilder.DropTable(
				name: "PublicationTags");

			migrationBuilder.DropTable(
				name: "PublicationUrls");

			migrationBuilder.DropTable(
				name: "RoleClaims");

			migrationBuilder.DropTable(
				name: "RoleLinks");

			migrationBuilder.DropTable(
				name: "RolePermission");

			migrationBuilder.DropTable(
				name: "SubmissionAuthors");

			migrationBuilder.DropTable(
				name: "SubmissionStatusHistory");

			migrationBuilder.DropTable(
				name: "UserAwards");

			migrationBuilder.DropTable(
				name: "UserClaims");

			migrationBuilder.DropTable(
				name: "UserDisallows");

			migrationBuilder.DropTable(
				name: "UserFileComments");

			migrationBuilder.DropTable(
				name: "UserLogins");

			migrationBuilder.DropTable(
				name: "UserRoles");

			migrationBuilder.DropTable(
				name: "UserTokens");

			migrationBuilder.DropTable(
				name: "WikiReferrals");

			migrationBuilder.DropTable(
				name: "ForumPollOptions");

			migrationBuilder.DropTable(
				name: "ForumTopics");

			migrationBuilder.DropTable(
				name: "GameGroups");

			migrationBuilder.DropTable(
				name: "Genres");

			migrationBuilder.DropTable(
				name: "GameRamAddressDomains");

			migrationBuilder.DropTable(
				name: "Flags");

			migrationBuilder.DropTable(
				name: "Tags");

			migrationBuilder.DropTable(
				name: "Publications");

			migrationBuilder.DropTable(
				name: "Awards");

			migrationBuilder.DropTable(
				name: "UserFiles");

			migrationBuilder.DropTable(
				name: "Roles");

			migrationBuilder.DropTable(
				name: "ForumPolls");

			migrationBuilder.DropTable(
				name: "Forums");

			migrationBuilder.DropTable(
				name: "Submissions");

			migrationBuilder.DropTable(
				name: "ForumCategories");

			migrationBuilder.DropTable(
				name: "GameRoms");

			migrationBuilder.DropTable(
				name: "GameSystemFrameRates");

			migrationBuilder.DropTable(
				name: "SubmissionRejectionReasons");

			migrationBuilder.DropTable(
				name: "Tiers");

			migrationBuilder.DropTable(
				name: "User");

			migrationBuilder.DropTable(
				name: "WikiPages");

			migrationBuilder.DropTable(
				name: "Games");

			migrationBuilder.DropTable(
				name: "GameSystems");
		}
	}
}
