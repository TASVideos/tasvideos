using System.Collections.Generic;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Data.SeedData;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class GameGenreImport
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var gameGenres = new HashSet<GameGenre>();

			var genreLookup = GenreSeedData.Genres
				.ToDictionary(tkey => tkey.DisplayName, tvalue => tvalue.Id);

			var data = (
				from m in legacySiteContext.Movies
				join s in legacySiteContext.Submissions on m.SubmissionId equals s.Id
				join gn in legacySiteContext.GameNames on s.GameNameId equals gn.Id
				join mc in legacySiteContext.MovieClass on m.Id equals mc.MovieId
				join c in legacySiteContext.ClassTypes on mc.ClassId equals c.Id
				where c.PositiveText.StartsWith("Genre")
				select new
				{
					GameId = gn.Id,
					GenreKey = c.PositiveText.Replace("Genre: ", "")
				})
				.Distinct()
				.ToList();

			foreach (var d in data)
			{
				gameGenres.Add(new GameGenre
				{
					GameId = d.GameId,
					GenreId = genreLookup[d.GenreKey]
				});
			}

			var gameGenreColumns = new[]
			{
				nameof(GameGenre.GameId),
				nameof(GameGenre.GenreId)
			};

			gameGenres.BulkInsert(connectionStr, gameGenreColumns, nameof(ApplicationDbContext.GameGenres));
		}
	}
}
