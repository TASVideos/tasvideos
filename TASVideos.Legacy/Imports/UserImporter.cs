using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;
using TASVideos.WikiEngine;


namespace TASVideos.Legacy.Imports
{
	public static class UserImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext, NesVideosForumContext legacyForumContext)
		{
			// TODO: roles
			// TODO: banned users, forum user_active status?
			// TODO: gender?

			var users = legacySiteContext.Users
				.OrderBy(u => u.Id)
				.ToList();

			var forumUsers = legacyForumContext.Users
				.OrderBy(u => u.UserId)
				.ToList();

			var wikiNoForum = users
				.Select(u => u.Name)
				.Except(forumUsers.Select(u => u.UserName))
				.ToList();

			var forumNoWiki = forumUsers
				.Select(u => u.UserName)
				.Except(users.Select(u => u.Name))
				.ToList();

			foreach (var legacyForumUser in forumUsers)
			{
				var newUser = new TASVideos.Data.Entity.User
				{
					UserName = legacyForumUser.UserName,
					NormalizedUserName = legacyForumUser.UserName.ToUpper(),
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyForumUser.RegDate),
					LegacyPassword = legacyForumUser.Password,
					EmailConfirmed = legacyForumUser.EmailTime != null,
					Email = legacyForumUser.Email
				};

				context.Users.Add(newUser);
			}

			context.SaveChanges();
		}
	}
}
