using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumTopicWatchImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosForumContext legacyForumContext)
		{
			var watches = legacyForumContext.TopicWatch
				.Select(t => new ForumTopicWatch
				{
					UserId = t.UserId,
					ForumTopicId = t.TopicId,
					IsNotified = t.NotifyStatus
				})
				.Distinct()
				.ToList();

			var columns = new[]
			{
				nameof(ForumTopicWatch.UserId),
				nameof(ForumTopicWatch.ForumTopicId),
				nameof(ForumTopicWatch.IsNotified)
			};

			watches.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.ForumTopicWatches));
		}
	}
}
