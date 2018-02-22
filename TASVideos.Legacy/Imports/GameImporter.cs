using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class GameImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var legacyGameNames = legacySiteContext.GameNames.ToList();
			var legacyClassTypes = legacySiteContext.ClassTypes.ToList();
			var legacyMovieClass = legacySiteContext.MovieClass.ToList();
			var legacySubmissions = legacySiteContext.Submissions
				.Select(s => new { s.Id, s.GameNameId })
				.ToList();

			var legacyPublications = legacySiteContext.Movies
				.Select(m => new { m.Id, m.SubmissionId })
				.ToList();

			var games = new List<Game>();
			foreach (var legacyGameName in legacyGameNames)
			{
				var game = new Game
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
				};

				games.Add(game);
			}

			// The legacy system did not strictly enforce a game for publications
			// but the new system demands fully cataloged publications
			// we want a placeholder game entry for publications that lack a game entry
			// And also for the placeholder rom because roms are also strictly enforced
			// to have a game
			games.Add(new Game
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
			});

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
		}
	}
}