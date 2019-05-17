using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class GameGroupImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var gameGroups = legacySiteContext.GameNameGroupNames
				.Select(g => new GameGroup
				{
					Id = g.Id,
					Name = g.Name,
					SearchKey = g.SearchKey
				})
				.ToList();

			var groupColumns = new[]
			{
				nameof(GameGroup.Id),
				nameof(GameGroup.Name),
				nameof(GameGroup.SearchKey)
			};

			gameGroups.BulkInsert(connectionStr, groupColumns, nameof(ApplicationDbContext.GameGroups));

			var gameGameGroups = legacySiteContext.GameNameGroups
				.Select(g => new GameGameGroup
				{
					GameId = g.GnId,
					GameGroupId = g.GroupId
				})
				.ToList();

			var gameGameGroupColumns = new[]
			{
				nameof(GameGameGroup.GameId),
				nameof(GameGameGroup.GameGroupId)
			};

			gameGameGroups.BulkInsert(connectionStr, gameGameGroupColumns, nameof(ApplicationDbContext.GameGameGroups));
		}
	}
}
