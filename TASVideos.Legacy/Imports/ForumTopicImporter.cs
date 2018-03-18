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
					t.Id, t.ForumId, t.Title, t.PosterId, t.Timestamp
				})
				.ToList()
				.Select(t => new ForumTopic
				{
					Id = t.Id,
					ForumId = t.ForumId,
					Title = t.Title,
					PosterId = t.PosterId, // TODO: do all these match up?
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					CreateUserName = "LegacyImport",
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					LastUpdateUserName = "LegacyImport"
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
				nameof(ForumTopic.LastUpdateUserName)
			};

			topics.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumTopics));
		}
    }
}
