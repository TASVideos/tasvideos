using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class PublicationMaintenanceLogImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var raw = legacySiteContext.MovieMaintenanceLog
				.ToList();

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
				.Select(l => new PublicationMaintenanceLog
				{
					Id = l.Id,
					UserId = userMap[l.UserId],
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
