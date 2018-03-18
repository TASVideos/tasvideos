using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumPostsImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var legacyPosts = legacyForumContext.Posts.ToList();
			var legacyPostText = legacyForumContext.PostsText.ToList();
			var users = context.Users.Select(u => new { u.Id, u.UserName }).ToList();

			var posts = new List<ForumPost>();
			foreach (var lp in legacyPosts)
			{
				try
				{
				var legacyPostsText = legacyPostText.SingleOrDefault(lpt => lpt.Id == lp.Id);
				if (legacyPostsText == null)
				{
					continue; // TODO: what's going on with these posts??
				}

				var user = users.SingleOrDefault(u => u.Id == lp.PosterId);
				var lastEditedUser = lp.LastUpdateUserName > 0
					? users.SingleOrDefault(u => u.Id == lp.LastUpdateUserName)
					: null;

				var post = new ForumPost
				{
					Id = lp.Id,
					TopicId = lp.TopicId,
					PosterId = lp.PosterId,
					IpAddress = lp.IpAddress,
					Subject = legacyPostsText?.Subject,
					Text = legacyPostsText?.Text,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(lp.Timestamp),
					LastUpdateTimeStamp = lp.LastUpdateTimestamp.HasValue
						? ImportHelper.UnixTimeStampToDateTime(lp.LastUpdateTimestamp.Value)
						: ImportHelper.UnixTimeStampToDateTime(lp.Timestamp),
					CreateUserName = user?.UserName ?? "Unknown",
					LastUpdateUserName = lastEditedUser?.UserName ?? lp.LastUpdateUserName?.ToString() ?? "Unknown"
				};

				posts.Add(post);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			var columns = new[]
			{
				nameof(ForumPost.Id),
				nameof(ForumPost.TopicId),
				nameof(ForumPost.PosterId),
				nameof(ForumPost.IpAddress),
				nameof(ForumPost.Subject),
				nameof(ForumPost.Text),
				nameof(ForumPost.CreateTimeStamp),
				nameof(ForumPost.CreateUserName),
				nameof(ForumPost.LastUpdateTimeStamp),
				nameof(ForumPost.LastUpdateUserName)
			};

			posts.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumPosts));
		}
	}
}
