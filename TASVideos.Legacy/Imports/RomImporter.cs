using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class RomImporter
	{
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var roms = legacySiteContext.Roms
				.Where(r => r.Type == "G")
				.Select(r => new GameRom
				{
					Id = r.Id,
					Md5 = r.Md5,
					Sha1 = r.Sha1,
					Name = r.Description,
					Type = RomTypes.Good,
					GameId = r.GameId,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow
				})
				.ToList();

			roms.Add(UnknownRom);

			var columns = new[]
			{
				nameof(GameRom.Id),
				nameof(GameRom.Md5),
				nameof(GameRom.Sha1),
				nameof(GameRom.Name),
				nameof(GameRom.Type),
				nameof(GameRom.GameId),
				nameof(GameRom.CreateTimeStamp),
				nameof(GameRom.LastUpdateTimeStamp)
			};

			roms.BulkInsert(connectionStr, columns, nameof(ApplicationDbContext.Roms));
		}

		// The legacy system barely used roms and they were never enforced, but the new system demands
		// fully cataloged publications, so let's create a placeholder ROM with the intent of filling in
		// this info eventually
		private static readonly GameRom UnknownRom = new GameRom
		{
			Id = -1,
			Md5 = "00000000000000000000000000000000",
			Sha1 = "0000000000000000000000000000000000000000",
			Name = "Unknown Rom",
			Type = RomTypes.Unknown,
			CreateUserName = "adelikat",
			LastUpdateUserName = "adelikat",
			CreateTimeStamp = DateTime.UtcNow,
			LastUpdateTimeStamp = DateTime.UtcNow,
			GameId = -1 // Placeholder game
		};
	}
}
