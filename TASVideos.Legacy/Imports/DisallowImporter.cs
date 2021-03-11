using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class DisallowImporter
	{
		public static void Import(NesVideosForumContext legacyForumContext)
		{
			var disallow = legacyForumContext.Disallows
				.ToList()
				.Select(d => d.DisallowUserName.Replace("*", "").Trim())
				.Distinct()
				.Select(d => new UserDisallow
				{
					RegexPattern = d,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow
				})
				.ToList();

			var columns = new[]
			{
				nameof(UserDisallow.RegexPattern),
				nameof(UserDisallow.CreateTimestamp),
				nameof(UserDisallow.LastUpdateTimestamp)
			};

			disallow.BulkInsert(columns, nameof(ApplicationDbContext.UserDisallows));
		}
	}
}
