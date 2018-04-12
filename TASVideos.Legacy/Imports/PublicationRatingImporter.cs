using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class PublicationRatingImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var ratings = legacySiteContext.MovieRating
				.Select(mr => new PublicationRating
				{
					UserId = mr.UserId,
					PublicationId = mr.MovieId,
					Type = mr.RatingName == "Entertainment"
						? PublicationRatingType.Entertainmnet
						: PublicationRatingType.TechQuality,
					Value = mr.Value
				});

			var columns = new[]
			{
				nameof(PublicationRating.UserId),
				nameof(PublicationRating.PublicationId),
				nameof(PublicationRating.Type),
				nameof(PublicationRating.Value),
			};

			ratings.BulkInsert(context, columns, nameof(ApplicationDbContext.PublicationRatings), SqlBulkCopyOptions.Default, 20000);
		}
	}
}
