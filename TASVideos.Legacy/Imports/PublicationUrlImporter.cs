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
			const string MirrorType = "A";
			const string Streaming = "J";

			var urls = legacySiteContext.MovieFiles
				.Where(f => f.Type == MirrorType || f.Type == Streaming)
				.Where(f => f.Movie != null)
				.Select(f => new PublicationUrl
				{
					PublicationId = f.Movie!.Id,
					Url = f.FileName,
					Type = f.Type == MirrorType ? PublicationUrlType.Mirror : PublicationUrlType.Streaming,
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
