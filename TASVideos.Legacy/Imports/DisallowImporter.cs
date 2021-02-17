using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class DisallowImporter
	{
		public static void Import(
			string connectionStr,
			NesVideosForumContext legacyForumContext)
		{
			var disallow = legacyForumContext.Disallows
				.ToList()
				.Select(d => d.DisallowUserName.Replace("*", "").Trim())
				.Distinct()
				.Select(d => new UserDisallow
				{
					RegexPattern = d,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow
				})
				.ToList();

			var columns = new[]
			{
				nameof(UserDisallow.RegexPattern),
				nameof(UserDisallow.CreateTimeStamp),
				nameof(UserDisallow.LastUpdateTimeStamp)
			};

			disallow.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.UserDisallows));
		}
	}
}
