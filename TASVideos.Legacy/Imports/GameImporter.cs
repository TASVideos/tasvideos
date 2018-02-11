using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Legacy.Imports
{
	public static class GameImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var legacyGameNames = legacySiteContext.GameNames.ToList();

			var gameQuery = new StringBuilder("SET IDENTITY_INSERT Games ON\n");

			gameQuery.Append(string.Concat(legacyGameNames.Select(gn =>
$@"
INSERT INTO Games
({nameof(Game.Id)}, {nameof(Game.Abbreviation)}, {nameof(Game.CreateTimeStamp)}, {nameof(Game.DisplayName)},
{nameof(Game.GoodName)}, {nameof(Game.LastUpdateTimeStamp)}, {nameof(Game.SearchKey)}, {nameof(Game.SystemId)}, {nameof(Game.YoutubeTags)})
VALUES({gn.Id}, '{gn.Abbreviation}', getutcdate(), '{gn.DisplayName.Replace("'", "''")}', '{gn.GoodName.Replace("'", "''")}', getutcdate(),
'{gn.SearchKey}', '{gn.SystemId}', '{gn.YoutubeTags.Replace("'", "''")}') ")));

			using (var sqlConnection = new SqlConnection(context.Database.GetDbConnection().ConnectionString))
			{
				sqlConnection.Open();
				using (var cmd = new SqlCommand
				{
					CommandText = gameQuery.ToString(),
					Connection = sqlConnection
				})
				{
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
