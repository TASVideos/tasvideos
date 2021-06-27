using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class PublicationFlagImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext)
		{
			var publicationFlags = legacySiteContext.MovieFlags
				.Where(mf => mf.FlagId != 3) // A.V.G.N
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

			publicationFlags.BulkInsert(columns, nameof(ApplicationDbContext.PublicationFlags));
		}
	}
}
