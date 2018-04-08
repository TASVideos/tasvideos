using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Awards;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public static class AwardImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			using (legacySiteContext.Database.BeginTransaction())
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
					.Select(a => new UserAward
					{
						AwardId = a.AwardId,
						UserId = a.UserId,
						Year = a.Year
					})
					.ToList();

				var publicationAwards = legacySiteContext.Awards
					.Where(a => a.MovieId > 0)
					.Select(a => new PublicationAward
					{
						AwardId = a.AwardId,
						PublicationId = a.MovieId,
						Year = a.Year
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

				awards.BulkInsert(context, awardColumns, nameof(ApplicationDbContext.Awards));
				userAwards.BulkInsert(context, userAwardColumns, nameof(ApplicationDbContext.UserAwards), SqlBulkCopyOptions.Default);
				publicationAwards.BulkInsert(context, pubAwardColumns, nameof(ApplicationDbContext.PublicationAwards), SqlBulkCopyOptions.Default);
			}
		}
	}
}
