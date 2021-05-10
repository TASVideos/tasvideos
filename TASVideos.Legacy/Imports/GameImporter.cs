using System;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal static class GameImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext)
		{
			var games = legacySiteContext.GameNames
				.Select(g => new Game
				{
					Id = g.Id,
					SystemId = g.SystemId,
					GoodName = ImportHelper.ConvertNotNullLatin1String(g.GoodName),
					DisplayName = ImportHelper.ConvertNotNullLatin1String(g.DisplayName),
					Abbreviation = ImportHelper.ConvertLatin1String(g.Abbreviation).NullIfWhiteSpace(),
					SearchKey = ImportHelper.ConvertLatin1String(g.SearchKey).NullIfWhiteSpace(),
					YoutubeTags = g.YoutubeTags,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					GameResourcesPage = ImportHelper.ConvertLatin1String(g.ResourceName).NullIfWhiteSpace()
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
				nameof(Game.CreateTimestamp),
				nameof(Game.LastUpdateTimestamp),
				nameof(Game.GameResourcesPage)
			};

			games.BulkInsert(columns, nameof(ApplicationDbContext.Games));
		}

		// The legacy system did not strictly enforce a game for publications
		// but the new system demands fully cataloged publications
		// we want a placeholder game entry for publications that lack a game entry
		// And also for the placeholder rom because roms are also strictly enforced
		// to have a game
		private static readonly Game UnknownGame = new ()
		{
			Id = -1,
			SystemId = 1, // Arbitrary, I'd rather not have a placeholder system because it could clutter a lot of parts of the code
			GoodName = "Unknown Game",
			DisplayName = "Unknown Game",
			Abbreviation = "Unknown",
			SearchKey = "Unknown",
			YoutubeTags = "Unknown",
			CreateUserName = "adelikat",
			LastUpdateUserName = "adelikat",
			CreateTimestamp = DateTime.UtcNow,
			LastUpdateTimestamp = DateTime.UtcNow
		};
	}
}
