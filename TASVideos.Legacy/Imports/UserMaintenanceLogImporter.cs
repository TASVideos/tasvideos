using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class UserMaintenanceLogImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var raw = legacySiteContext.UserMaintenanceLogs.ToList();

			var siteUsers = legacySiteContext.Users
				.Select(u => new { u.Id, u.Name })
				.Distinct()
				.ToList();

			var users = context.Users.ToList();

			// TODO: awards and other things have to do this site to forum id mapping, do it once and pass it in as a dictionary
			var userMap = (from lu in siteUsers
					join u in users on lu.Name.ToLower() equals u.UserName.ToLower()
					select new
					{
						SiteId = lu.Id,
						ForumId = u.Id
					})
				.ToDictionary(tkey => tkey.SiteId, tvalue => tvalue.ForumId);

			var logs = raw
				.Where(r => userMap.ContainsKey(r.UserId))
				.Where(r => userMap.ContainsKey(r.EditorId))
				.Select(l => new UserMaintenanceLog
				{
					Id = l.Id,
					UserId = userMap[l.UserId],
					EditorId = userMap[l.EditorId],
					TimeStamp = ImportHelper.UnixTimeStampToDateTime(l.TimeStamp),
					Log = l.Content
				})
				.ToList();

			var columns = new[]
			{
				nameof(UserMaintenanceLog.Id),
				nameof(UserMaintenanceLog.UserId),
				nameof(UserMaintenanceLog.EditorId),
				nameof(UserMaintenanceLog.TimeStamp),
				nameof(UserMaintenanceLog.Log)
			};

			logs.BulkInsert(columns, nameof(ApplicationDbContext.UserMaintenanceLogs));
		}
	}
}
