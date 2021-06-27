﻿using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	internal static class ForumTopicWatchImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
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

			watches.BulkInsert(columns, nameof(ApplicationDbContext.ForumTopicWatches));
		}
	}
}
