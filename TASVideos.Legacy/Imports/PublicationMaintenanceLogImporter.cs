using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class PublicationMaintenanceLogImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext, IReadOnlyDictionary<int, int> userIdMapping)
		{
			var raw = legacySiteContext.MovieMaintenanceLog
				.ToList();

			var logs = raw
				.Select(l => new PublicationMaintenanceLog
				{
					Id = l.Id,
					UserId = userIdMapping[l.UserId],
					PublicationId = l.MovieId,
					TimeStamp = ImportHelper.UnixTimeStampToDateTime(l.TimeStamp),
					Log = l.Content
				})
				.ToList();

			var columns = new[]
			{
				nameof(PublicationMaintenanceLog.Id),
				nameof(PublicationMaintenanceLog.UserId),
				nameof(PublicationMaintenanceLog.PublicationId),
				nameof(PublicationMaintenanceLog.TimeStamp),
				nameof(PublicationMaintenanceLog.Log)
			};

			logs.BulkInsert(columns, nameof(ApplicationDbContext.PublicationMaintenanceLogs));
		}
	}
}
