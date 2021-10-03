using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class UserMaintenanceLogImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext, IReadOnlyDictionary<int, int> userIdMapping)
		{
			var raw = legacySiteContext.UserMaintenanceLogs.ToList();

			var logs = raw
				.Where(r => userIdMapping.ContainsKey(r.UserId))
				.Where(r => userIdMapping.ContainsKey(r.EditorId))
				.Select(l => new UserMaintenanceLog
				{
					Id = l.Id,
					UserId = userIdMapping[l.UserId],
					EditorId = userIdMapping[l.EditorId],
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
