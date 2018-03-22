using System;
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
			var games = legacySiteContext.GameNames
				.Select(g => new Game
				{
					Id = g.Id,
					SystemId = g.SystemId,
					GoodName = g.GoodName,
					DisplayName = g.DisplayName,
					Abbreviation = g.Abbreviation,
					SearchKey = g.SearchKey,
					YoutubeTags = g.YoutubeTags,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow
				})
				.ToList();

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
	}
}