using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Awards;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class AwardImporter
	{
		public static void Import(
			NesVideosSiteContext legacySiteContext,
			IReadOnlyDictionary<int, int> userIdMapping)
		{
			var awards = legacySiteContext.AwardClasses
				.Select(ac => new Award
				{
					Id = ac.Id,
					Type = ac.Class == "movie"
						? AwardType.Movie
						: AwardType.User,
					ShortName = ac.ShortName,
					Description = ac.Description
				})
				.ToList();

			var userAwards = legacySiteContext.Awards
				.Where(a => a.UserId > 0)
				.Select(a => new
				{
					a.AwardId,
					UserId = userIdMapping[a.UserId],
					Year = 2000 + a.Year
				})
				.ToList();

			var publicationAwards = legacySiteContext.Awards
				.Where(a => a.MovieId > 0)
				.Select(a => new PublicationAward
				{
					AwardId = a.AwardId,
					PublicationId = a.MovieId,
					Year = 2000 + a.Year
				})
				.ToList();

			var awardColumns = new[]
			{
				nameof(Award.Id),
				nameof(Award.Type),
				nameof(Award.ShortName),
				nameof(Award.Description)
			};

			var userAwardColumns = new[]
			{
				nameof(UserAward.UserId),
				nameof(UserAward.AwardId),
				nameof(UserAward.Year)
			};

			var pubAwardColumns = new[]
			{
				nameof(PublicationAward.PublicationId),
				nameof(PublicationAward.AwardId),
				nameof(PublicationAward.Year)
			};

			awards.BulkInsert(awardColumns, nameof(ApplicationDbContext.Awards));
			userAwards.BulkInsert(userAwardColumns, nameof(ApplicationDbContext.UserAwards));
			publicationAwards.BulkInsert(pubAwardColumns, nameof(ApplicationDbContext.PublicationAwards));
		}
	}
}
