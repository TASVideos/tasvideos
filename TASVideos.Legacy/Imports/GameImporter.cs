using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;

namespace TASVideos.Legacy.Imports
{
	public static class GameImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			List<GameName> legacyGameNames;
			List<ClassTypeDto> legacyClassTypes;
			List<MovieClass> legacyMovieClass;
			List<PublicationDto> legacyPublications;

			using (legacySiteContext.Database.BeginTransaction())
			{
				legacyGameNames = legacySiteContext.GameNames.ToList();

				legacyClassTypes = legacySiteContext.ClassTypes
					.Where(c => c.PositiveText.StartsWith("Genre"))
					.Select(c => new ClassTypeDto
					{
						Id = c.Id,
						PositiveText = c.PositiveText.Replace("Genre: ", "")
					})
					.ToList();

				legacyMovieClass = legacySiteContext.MovieClass.ToList();

				legacyPublications = legacySiteContext.Movies
					.Where(m => m.Submission.GameNameId.HasValue)
					.Select(m => new PublicationDto
					{
						Id = m.Id,
						GameNameId = m.Submission.GameNameId.Value
					})
					.ToList();
			}

			var genres = context.Genres.ToList();

			var games = new List<Game>();
			var gameGenres = new HashSet<GameGenre>();

			var lgn = legacyGameNames
				.Select(g => new
				{
					g.Id,
					g.SystemId,
					g.GoodName,
					g.DisplayName,
					g.Abbreviation,
					g.SearchKey,
					g.YoutubeTags,
					GameGenres =
						(from p in legacyPublications
						 join mc in legacyMovieClass on p.Id equals mc.MovieId
						 join c in legacyClassTypes on mc.ClassId equals c.Id
						 where p.GameNameId == g.Id
						 select c)
						.Distinct()
						.ToList()
				})
				.ToList();

			foreach (var legacyGameName in lgn)
			{
				foreach (var lgg in legacyGameName.GameGenres)
				{
					gameGenres.Add(new GameGenre
					{
						GenreId = genres.Single(g => g.DisplayName == lgg.PositiveText).Id,
						GameId = legacyGameName.Id
					});
				}

				games.Add(new Game
				{
					Id = legacyGameName.Id,
					SystemId = legacyGameName.SystemId,
					GoodName = legacyGameName.GoodName,
					DisplayName = legacyGameName.DisplayName,
					Abbreviation = legacyGameName.Abbreviation,
					SearchKey = legacyGameName.SearchKey,
					YoutubeTags = legacyGameName.YoutubeTags,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow
				});
			}

			games.Add(UnknownGame);

			var columns = new[]
			{
				nameof(Game.Id),
				nameof(Game.SystemId),
				nameof(Game.GoodName),
				nameof(Game.DisplayName),
				nameof(Game.Abbreviation),
				nameof(Game.SearchKey),
				nameof(Game.YoutubeTags),
				nameof(Game.CreateTimeStamp),
				nameof(Game.LastUpdateTimeStamp)
			};

			games.BulkInsert(context, columns, nameof(ApplicationDbContext.Games));

			var gameGenreColumns = new[]
			{
				nameof(GameGenre.GameId),
				nameof(GameGenre.GenreId)
			};

			gameGenres.BulkInsert(context, gameGenreColumns, nameof(ApplicationDbContext.GameGenres));
		}

		// The legacy system did not strictly enforce a game for publications
		// but the new system demands fully cataloged publications
		// we want a placeholder game entry for publications that lack a game entry
		// And also for the placeholder rom because roms are also strictly enforced
		// to have a game
		private static readonly Game UnknownGame = new Game
		{
			Id = -1,
			SystemId = 1, // Arbitruary, I'd rather not have a placeholder system because it could clutter a lot of parts of the code
			GoodName = "Unknown Game",
			DisplayName = "Unknown Game",
			Abbreviation = "Unknown",
			SearchKey = "",
			YoutubeTags = "",
			CreateUserName = "adelikat",
			LastUpdateUserName = "adelikat",
			CreateTimeStamp = DateTime.UtcNow,
			LastUpdateTimeStamp = DateTime.UtcNow,
		};

		private class PublicationDto
		{
			public int Id { get; set; }
			public int GameNameId { get; set; }
		}

		private class ClassTypeDto
		{
			public int Id { get; set; }
			public string PositiveText { get; set; }
		}
	}
}