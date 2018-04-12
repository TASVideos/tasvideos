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
				.Where(t => t.TopicMovedId == 0) // Topic moved id indicates it is not a valid topic anymore, so simply filter them out
				.Select(t => new
				{
					t.Id, t.ForumId, t.Title, t.PosterId, t.Timestamp, t.Views, t.Type,
					Author = t.PosterId > 0 ? t.Poster.UserName : "Unknown"
				})
				.ToList()
				.Select(t => new ForumTopic
				{
					Id = t.Id,
					ForumId = t.ForumId,
					Title = ImportHelper.FixString(t.Title),
					PosterId = t.PosterId > 0 // There's one record that is 0 we want to change to -1
						? t.PosterId  // TODO: Some of these do not match up to known users! We should at least put -1 here
						: -1,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					CreateUserName = t.Author,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(t.Timestamp),
					LastUpdateUserName = "LegacyImport",
					Views = t.Views,
					Type = (ForumTopicType)t.Type
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
				nameof(ForumTopic.Views),
				nameof(ForumTopic.Type)
			};

			topics.BulkInsert(context, columns, nameof(ApplicationDbContext.ForumTopics));
		}
    }
}
