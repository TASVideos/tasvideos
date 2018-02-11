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
			var systems = context.GameSystems.ToList();

			foreach (var legacyGameName in legacyGameNames)
			{
				
					var game = new Game
					{
						System = systems.Single(s => s.Id == legacyGameName.SystemId),
						GoodName = legacyGameName.GoodName,
						DisplayName = legacyGameName.DisplayName,
						Abbreviation = legacyGameName.Abbreviation,
						SearchKey = legacyGameName.SearchKey,
						YoutubeTags = legacyGameName.YoutubeTags
					};

					context.Games.Add(game);
			}

			context.SaveChanges();
		}
	}
}
