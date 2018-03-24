using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy.Data.Forum.Entity
{
    public static class ForumTopicImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var topics = legacyForumContext
				.Topics
				.Select(t => new
				{
					t.Id, t.ForumId, t.Title, t.PosterId, t.Timestamp, t.Views, Author = t.PosterId > 0 ? t.Poster.UserName : "Unknown"
				})
				.ToList()
				.Select(t => new ForumTopic
				{
					Id = t.Id,
					ForumId = t.ForumId,
					Title = t.Title,
					PosterId = t.PosterId > 0 // There's one record that is 0 we want to change to -1
						? t.PosterId  // TODO: Some of these do not match up to known users! We should at least put -1 here
						: -1,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					CreateUserName = t.Author,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					LastUpdateUserName = "LegacyImport",
					Views = t.Views
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumTopic.Id),
				nameof(ForumTopic.ForumId),
				nameof(ForumTopic.Title),
				nameof(ForumTopic.PosterId),
				nameof(ForumTopic.CreateTimeStamp),
				nameof(ForumTopic.LastUpdateTimeStamp),
				nameof(ForumTopic.CreateUserName),
				nameof(ForumTopic.LastUpdateUserName),
				nameof(ForumTopic.Views)
			};

			topics.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumTopics));
		}
    }
}
