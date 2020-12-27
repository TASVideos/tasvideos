using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class PublicationUrlImporter
	{
		public static void Import(
			string connectionStr,
			NesVideosSiteContext legacySiteContext)
		{
			const string mirrorType = "A";
			const string streaming = "J";

			var urls = legacySiteContext.MovieFiles
				.Where(f => f.Type == mirrorType || f.Type == streaming)
				.Where(f => f.Movie != null)
				.Select(f => new PublicationUrl
				{
					PublicationId = f.Movie!.Id,
					Url = f.FileName,
					Type = f.Type == mirrorType ? PublicationUrlType.Mirror : PublicationUrlType.Streaming,
					CreateTimeStamp = DateTime.Now,
					LastUpdateTimeStamp = DateTime.Now
				})
				.ToList();

			var columns = new[]
			{
				nameof(PublicationUrl.PublicationId),
				nameof(PublicationUrl.Url),
				nameof(PublicationUrl.Type),
				nameof(PublicationUrl.CreateTimeStamp),
				nameof(PublicationUrl.LastUpdateTimeStamp)
			};

			urls.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.PublicationUrls));
		}
	}
}
