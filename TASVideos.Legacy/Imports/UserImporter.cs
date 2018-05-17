using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class UserImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// TODO:
			// forum user_active status?
			// import forum users that have no wiki, but check if they are forum banned
			// gender?
			// timezone
			// user_avatar_type ?
			// mood avatars
			// TODO: what to do about these??
			//var wikiNoForum = legacyUsers
			//	.Select(u => u.Name)
			//	.Except(legacyForumUsers.Select(u => u.UserName))
			//	.ToList();
			var legacyUsers = legacySiteContext.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.ToList();

			var users = legacyForumContext.Users
				.Where(u => u.UserName != "Anonymous")
				.Select(u => new
				{
					u.UserId,
					u.UserName,
					u.RegDate,
					u.Password,
					u.EmailTime,
					u.PostCount,
					u.Email,
					u.Avatar,
					u.From,
					u.Signature,
					u.PublicRatings
				})
				.ToList()
				.Select(u => new User
				{
					Id = u.UserId,
					UserName = ImportHelper.FixString(u.UserName),
					NormalizedUserName = u.UserName.ToUpper(),
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(u.RegDate),
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(u.RegDate), // TODO
					LegacyPassword = u.Password,
					EmailConfirmed = u.EmailTime != null || u.PostCount > 0,
					Email = u.Email,
					NormalizedEmail = u.Email.ToUpper(),
					CreateUserName = "Automatic Migration",
					PasswordHash = "",
					Avatar = u.Avatar,
					From = u.From,
					Signature = ImportHelper.FixString(u.Signature),
					PublicRatings = u.PublicRatings
				})
				.ToList();

			var roles = context.Roles.ToList();

			var userRoles = new List<UserRole>();

			var joinedUsers = from user in users
					join su in legacyUsers on user.UserName equals su.Name into lsu
					from su in lsu.DefaultIfEmpty()
					select new { User = user, SiteUser = su };

			foreach (var user in joinedUsers)
			{
				if (user.SiteUser != null)
				{
					// not having user means they are effectively banned
					// limited = Limited User
					if (user.SiteUser.UserRoles.Any(ur => ur.Role.Name == "user")
						&& user.SiteUser.UserRoles.All(ur => ur.Role.Name != "admin")) // There's no point in adding these roles to admins, they have these perms anyway
					{
						userRoles.Add(new UserRole
						{
							RoleId = roles.Single(r => r.Name == SeedRoleNames.EditHomePage).Id,
							UserId = user.User.Id
						});

						if (user.SiteUser.UserRoles.All(ur => ur.Role.Name != "limited"))
						{
							context.UserRoles.Add(new UserRole
							{
								RoleId = roles.Single(r => r.Name == SeedRoleNames.SubmitMovies).Id,
								UserId = user.User.Id
							});
						}
					}

					foreach (var userRole in user.SiteUser.UserRoles.Select(ur => ur.Role)
						.Where(r => r.Name != "user" && r.Name != "limited"))
					{
						var role = GetRoleFromLegacy(userRole.Name, roles);
						if (role != null)
						{
							context.UserRoles.Add(new UserRole
							{
								RoleId = role.Id,
								UserId = user.User.Id
							});
						}
					}
				}
				else
				{
					// TODO: check any kind of active/ban forum status if none, then give them homepage and submit rights
				}
			}

			users.Add(new User
			{
				Id = -1,
				UserName = "Unknown User",
				NormalizedUserName = "UNKNOWN USER",
				Email = "",
				EmailConfirmed = true,
				LegacyPassword = ""
			});

			// Some published authors that have no forum account
			// Note that by having no password nor legacy password they effectively can not log in without a database change
			// I think this is correct since these are not active users
			var portedPlayerNames = new[]
			{
				"Morimoto",
				"Tokushin",
				"Yy",
				"Mathieu P",
				"Linnom",
				"Mclaud2000",
				"Ryosuke",
				"JuanPablo",
				"qcommand",
				"Mana."
			};

			var portedPlayers = portedPlayerNames.Select(p => new User
			{
				UserName = p,
				NormalizedUserName = p.ToUpper(),
				Email = $"imported{p}@tasvideos.org",
				NormalizedEmail = $"imported{p}@tasvideos.org".ToUpper(),
				CreateTimeStamp = DateTime.UtcNow,
				LastUpdateTimeStamp = DateTime.UtcNow
			});

			var userColumns = new[]
			{
				nameof(User.Id),
				nameof(User.UserName),
				nameof(User.NormalizedUserName),
				nameof(User.CreateTimeStamp),
				nameof(User.LegacyPassword),
				nameof(User.EmailConfirmed),
				nameof(User.Email),
				nameof(User.NormalizedEmail),
				nameof(User.CreateUserName),
				nameof(User.PasswordHash),
				nameof(User.AccessFailedCount),
				nameof(User.LastUpdateTimeStamp),
				nameof(User.LockoutEnabled),
				nameof(User.PhoneNumberConfirmed),
				nameof(User.TwoFactorEnabled),
				nameof(User.Avatar),
				nameof(User.From),
				nameof(User.Signature),
				nameof(User.PublicRatings)
			};

			var userRoleColumns = new[]
			{
				nameof(UserRole.UserId),
				nameof(UserRole.RoleId)
			};

			users.BulkInsert(context, userColumns, "[User]");
			userRoles.BulkInsert(context, userRoleColumns, "[UserRoles]");

			var playerColumns = userColumns.Where(p => p != nameof(User.Id)).ToArray();
			portedPlayers.BulkInsert(context, playerColumns, "[User]");
		}

		private static Role GetRoleFromLegacy(string role, IEnumerable<Role> roles)
		{
			switch (role.ToLower())
			{
				default:
					return null;
				case "editor":
					return roles.Single(r => r.Name == SeedRoleNames.Editor);
				case "vestededitor":
					return roles.Single(r => r.Name == SeedRoleNames.VestedEditor);
				case "publisher":
					return roles.Single(r => r.Name == SeedRoleNames.Publisher);
				case "seniorpublisher":
					return roles.Single(r => r.Name == SeedRoleNames.SeniorPublisher);
				case "judge":
					return roles.Single(r => r.Name == SeedRoleNames.Judge);
				case "seniorjudge":
					return roles.Single(r => r.Name == SeedRoleNames.SeniorJudge);
				case "adminassistant":
					return roles.Single(r => r.Name == SeedRoleNames.AdminAssistant);
				case "admin":
					return roles.Single(r => r.Name == SeedRoleNames.Admin);
			}
		}
	}
}
