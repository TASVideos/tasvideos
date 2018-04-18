using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public static class PublicationFlagImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var publicationFlags = legacySiteContext.MovieFlags
				.Where(mf => mf.FlagId != 3) // AVGN
				.Select(mf => new PublicationFlag
				{
					PublicationId = mf.MovieId,
					FlagId = mf.FlagId
				})
				.ToList();

			var columns = new[]
			{
				nameof(PublicationFlag.PublicationId),
				nameof(PublicationFlag.FlagId)
			};

			publicationFlags.BulkInsert(context, columns, nameof(ApplicationDbContext.PublicationFlags));
		}
	}
}
