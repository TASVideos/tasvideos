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
			string connectionStr,
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

				var userAwardsDto = legacySiteContext.Awards
					.Where(a => a.UserId > 0)
					.Select(a => new
					{
						a.AwardId,
						a.User.Name,
						Year = 2000 + a.Year
					})
					.ToList();

				var usersWithAwards = userAwardsDto
					.Select(u => u.Name)
					.Distinct()
					.ToList();

				// Match the user by username to get the user id (legacy awards are off site user id, but the new system uses the forum user id)
				var users = context.Users
					.Where(u => usersWithAwards.Contains(u.UserName))
					.Select(u => new { u.Id, u.UserName })
					.ToList();

				var userAwards = (from ua in userAwardsDto
					join u in users on ua.Name.ToLower() equals u.UserName.ToLower()
					select new UserAward
					{
						AwardId = ua.AwardId,
						Year = ua.Year,
						UserId = u.Id
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

				awards.BulkInsert(connectionStr, awardColumns, nameof(ApplicationDbContext.Awards));
				userAwards.BulkInsert(connectionStr, userAwardColumns, nameof(ApplicationDbContext.UserAwards), SqlBulkCopyOptions.Default);
				publicationAwards.BulkInsert(connectionStr, pubAwardColumns, nameof(ApplicationDbContext.PublicationAwards), SqlBulkCopyOptions.Default);
			}
		}
	}
}
