using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class PublicationRatingImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext, IReadOnlyDictionary<int, int> userIdMapping)
		{
			// TODO: there are a lot of ratings for users with no forum account, these votes are getting lost here
			var raw = legacySiteContext.MovieRating
				.Select(mr => new
				{
					mr.UserId,
					PublicationId = mr.MovieId,
					mr.RatingName,
					mr.Value
				})
				.ToList();

			// GroupBy shenanigans is to tease out duplicates caused by consolidating user accounts
			// that voted on the same movie with multiple accounts
			var ratingsHack = raw
				.Where(r => userIdMapping.ContainsKey(r.UserId))
				.Select(mr => new PublicationRating
				{
					UserId = userIdMapping[mr.UserId],
					PublicationId = mr.PublicationId,
					Type = mr.RatingName == "Entertainment"
						? PublicationRatingType.Entertainment
						: PublicationRatingType.TechQuality,
					Value = (double)mr.Value
				}).ToList();

			var ratings = ratingsHack
				.GroupBy(g => new { g.UserId, g.PublicationId, g.Type }, gv => gv)
				.Select(g => g.First())
				.ToList();

			var columns = new[]
			{
				nameof(PublicationRating.UserId),
				nameof(PublicationRating.PublicationId),
				nameof(PublicationRating.Type),
				nameof(PublicationRating.Value)
			};

			ratings.BulkInsert(columns, nameof(ApplicationDbContext.PublicationRatings));
		}
	}
}
