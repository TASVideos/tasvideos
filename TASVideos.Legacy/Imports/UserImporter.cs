using System.Collections.Generic;
using System.Linq;

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
			// gender?


			var users = legacySiteContext.Users
				.OrderBy(u => u.Id)
				.ToList();

			var forumUsers = legacyForumContext.Users
				.OrderBy(u => u.UserId)
				.ToList();

			var luserRoles = legacySiteContext.UserRoles.ToList();
			var lroles = legacySiteContext.Roles.ToList();

			var roles = context.Roles.ToList();

			// TODO: what to do about these??
			var wikiNoForum = users
				.Select(u => u.Name)
				.Except(forumUsers.Select(u => u.UserName))
				.ToList();

			foreach (var legacyForumUser in forumUsers)
			{
				if (legacyForumUser.UserName == "Anonymous")
				{
					continue;
				}

				var newUser = new TASVideos.Data.Entity.User
				{
					UserName = legacyForumUser.UserName,
					NormalizedUserName = legacyForumUser.UserName.ToUpper(),
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyForumUser.RegDate),
					LegacyPassword = legacyForumUser.Password,
					EmailConfirmed = legacyForumUser.EmailTime != null,
					Email = legacyForumUser.Email,
					NormalizedEmail = legacyForumUser.Email.ToUpper(),
					CreateUserName = "Automatic Migration",
					PasswordHash = "",
				};

				context.Users.Add(newUser);


				var legacySiteUser = users.SingleOrDefault(u => u.Name == legacyForumUser.UserName);

				if (legacySiteUser != null)
				{
					var userRoles = (from lr in luserRoles
									join r in lroles on lr.RoleId equals r.Id
									where lr.UserId == legacySiteUser.Id
									select r)
									.ToList();

					// user = banned User
					// limited = Limited User
					if (!userRoles.Select(ur => ur.Name).Contains("user") && !userRoles.Select(ur => ur.Name).Contains("limited"))
					{
						context.UserRoles.Add(new UserRole
						{
							Role = RoleSeedData.SubmitMovies,
							User = newUser
						});
					}

					if (!userRoles.Select(ur => ur.Name).Contains("user"))
					{
						context.UserRoles.Add(new UserRole
						{
							Role = RoleSeedData.EditHomePage,
							User = newUser
						});
					}

					foreach (var userRole in userRoles
						.Where(r => r.Name != "user" && r.Name != "limited"))
					{
						var role = GetRoleFromLegacy(userRole.Name, roles);
						if (role != null)
						{
							context.UserRoles.Add(new UserRole
							{
								Role = role,
								User = newUser
							});
						}
					}
				}
			}

			context.SaveChanges();
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
