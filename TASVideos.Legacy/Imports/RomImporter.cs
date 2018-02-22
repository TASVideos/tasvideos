using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class RomImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var legacyRoms = legacySiteContext.Roms.Where(r => r.Type == "G").ToList();
			var roms = new List<GameRom>();

			foreach (var legacyRom in legacyRoms)
			{
				var rom = new GameRom
				{
					Id = legacyRom.Id,
					Md5 = legacyRom.Md5,
					Sha1 = legacyRom.Sha1,
					Name = legacyRom.Description,
					Type = RomTypes.Good,
					GameId = legacyRom.GameId,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp = DateTime.UtcNow
				};

				roms.Add(rom);
			}

			// The legacy system barely used roms and they were never enforced, but the new system demands
			// fully cataloged publications, so let's create a placeholder ROM with the intent of filling in
			// this info eventually
			roms.Add(new GameRom
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
			});

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

			roms.BulkInsert(context, columns, nameof(ApplicationDbContext.Roms));
		}
	}
}
